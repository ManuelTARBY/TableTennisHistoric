using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services;

namespace TableTennisHistoric.Pages
{
    public class MatchesModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;
        public List<TableTennisMatch> Matches { get; set; } = new();
        public List<MatchDTO> MatchesDTO { get; set; } = new();
        private MatchService _matchService { get; set; }
        private PlayerService _playerService { get; set; }
        private CompetitionService _competitionService;

        public MatchesModel(TableTennisHistoricDbContext context, MatchService matchService, PlayerService playerService, CompetitionService competitionService)
        {
            _context = context;
            _matchService = matchService;
            _playerService = playerService;
            _competitionService = competitionService;
        }

        // SelectLists
        public IEnumerable<SelectListItem> CompetitionCoefficients { get; set; }
        public IEnumerable<SelectListItem> Stages { get; set; }
        public IEnumerable<SelectListItem> Opponents { get; set; }

        // Form model
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public DateTime Date_match { get; set; } = DateTime.Today;

            [Required]
            public int CompetitionCoefficientId { get; set; }
            public int? StageId { get; set; } = null;

            [Required]
            public int OpponentId { get; set; }

            [Range(0, 4000)]
            public decimal My_points_at_match { get; set; }

            [Range(0, 4000)]
            public decimal Opponent_points_at_match { get; set; }

            [Required]
            public TableTennisMatch.MatchResult? Result { get; set; }
            public List<int?> SetMy { get; set; } = new(new int?[5]);
            public List<int?> SetOpp { get; set; } = new(new int?[5]);
            public string? Comment { get; set; } = null;
        }
      

        public async Task OnGetAsync()
        {
            // Récupère les données pour le formulaire
            await LoadSelectListsAsync();

        }

        public async Task<List<MatchDTO>> GetAllMatches()
        {
            return await _context.TableTennisMatch
                // Compétition
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Competition)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Season)

                // Adversaire + clubs par saison
                .Include(m => m.Opponent)
                    .ThenInclude(p => p.PlayerSeasons)
                        .ThenInclude(pc => pc.Club)

                .OrderByDescending(m => m.Date_match)
                .ThenByDescending(m => m.Id)

                .Select(m => new MatchDTO
                {
                    Id = m.Id,
                    Date_of_match = m.Date_match,

                    Competition = m.CompetitionCoefficient.Competition.Name,
                    Coefficient = m.CompetitionCoefficient.Coefficient,

                    OpponnentId = m.OpponentId,
                    Opponent_first_name = m.Opponent.First_name,
                    Opponent_last_name = m.Opponent.Last_name,
                    Opponent_full_name = m.Opponent.First_name + " " + m.Opponent.Last_name,

                    Opponent_club = m.Opponent.PlayerSeasons
                        .Where(pc => pc.SeasonId == m.CompetitionCoefficient.SeasonId)
                        .Select(pc => pc.Club.Name)
                        .FirstOrDefault(),

                    Opponent_points_at_match = m.Opponent_points_at_match,
                    Comment = m.Comment,

                    Result = Enum.Parse<MatchDTO.MatchResult>(m.Result.ToString()),
                })
                .ToListAsync();
        }

        private async Task LoadSelectListsAsync()
        {
            // Récupérer les matchs
            MatchesDTO = await GetAllMatches();

            SeasonService seasonService = new SeasonService(_context);
            Season currentSeason = await seasonService.GetCurrentSeasonAsync();

            CompetitionCoefficients = await _context.CompetitionCoefficient
                .Include(cc => cc.Competition)
                .Where(cc => cc.Competition != null
                      && cc.SeasonId == currentSeason.Id)
                        .Select(cc => new SelectListItem
                        {
                            Value = cc.Id.ToString(),
                            Text = cc.Competition.Name
                        })
                        .ToListAsync();

            Opponents = await _context.Player
                .Where(p => p.Id != 1)
                .OrderBy(p => p.First_name)
                .ThenBy(p => p.Last_name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .ToListAsync();

            Stages = await _context.Stage
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            // Load required navigation properties
            var competitionCoefficient =
                await _competitionService.GetCompetitionCoefficientByIdAsync(Input.CompetitionCoefficientId);

            var opponent = await _context.Player.FindAsync(Input.OpponentId);

            if (competitionCoefficient == null || opponent == null)
            {
                ModelState.AddModelError("", "Données invalides.");
                await LoadSelectListsAsync();
                return Page();
            }

            int? realStageId = Input.StageId;

            if (competitionCoefficient != null)
            {
                var competition = await _context.Competition
                    .FirstOrDefaultAsync(c => c.Id == competitionCoefficient.CompetitionId);
            }

            var match = new TableTennisMatch
            {
                Date_match = DateOnly.FromDateTime(Input.Date_match),
                CompetitionCoefficientId = Input.CompetitionCoefficientId,
                StageId = realStageId,
                OpponentId = Input.OpponentId,
                My_points_at_match = Input.My_points_at_match,
                Opponent_points_at_match = Input.Opponent_points_at_match,
                Result = Input.Result ?? TableTennisMatch.MatchResult.F,
                CompetitionCoefficient = competitionCoefficient,
                Opponent = opponent,
                Comment = Input.Comment != "" ? Input.Comment : null
            };

            _context.TableTennisMatch.Add(match);
            await _context.SaveChangesAsync();

            if (Input.SetMy.Any(s => s.HasValue) || Input.SetOpp.Any(s => s.HasValue))
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Input.SetMy[i].HasValue && Input.SetOpp[i].HasValue)
                    {
                        var set = new MatchSet
                        { 
                            MatchId = match.Id,
                            Player1Score = Input.SetMy[i]!.Value,
                            Player2Score = Input.SetOpp[i]!.Value,
                            SetNumber = i + 1,
                            Match = match
                        };

                        _context.MatchSet.Add(set);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

    }
}
