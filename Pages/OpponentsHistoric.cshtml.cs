using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Pages
{
    public class OpponentsHistoric : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;
        [BindProperty(SupportsGet = true)]
        public int? SelectedOpponentId { get; set; }
        public List<MatchDTO> Matches { get; set; } = new List<MatchDTO>();
        public SelectList OpponentsSelectList { get; set; } = new SelectList(Enumerable.Empty<Player>(), "Id", "FullName");
        public decimal? VictoryPercentage { get; set; }
        public decimal CounterOfVictory { get; set; } = 0;
        public decimal CounterOfDefeat { get; set; } = 0;
        public decimal CounterOfWithdraw { get; set; } = 0;
        public string? SeasonOfLastMatch { get; set; } = null;

        public OpponentsHistoric(TableTennisHistoricDbContext context)
        {
            _context = context;
        }
        public async Task OnGetAsync()
        {
            await LoadOpponentsAsync();

            if (SelectedOpponentId.HasValue)
            {
                Matches = await GetMatchesByOpponentAsync();

                if (Matches.Count > 0)
                {
                    CounterOfVictory = Matches.Count(m => m.Result == MatchDTO.MatchResult.V);
                    CounterOfDefeat = Matches.Count(m => m.Result == MatchDTO.MatchResult.D);
                    CounterOfWithdraw = Matches.Count(m => m.Result == MatchDTO.MatchResult.F);
                    VictoryPercentage = Math.Round(CounterOfVictory / (Matches.Count - CounterOfWithdraw) * 100m, 2);
                }
            }
        }

        private async Task LoadOpponentsAsync()
        {
            var opponents = await _context.Player
                .Where(p => p.Is_me == false)
                .OrderBy(p => p.First_name)
                .ThenBy(p => p.Last_name)
                .Select(p => new
                {
                    Id = p.Id,
                    FullName = $"{p.First_name} {p.Last_name}"
                })
                .ToListAsync();

            OpponentsSelectList = new SelectList(opponents, "Id", "FullName");
        }

        public async Task<IActionResult> OnPostSelectOpponentAsync()
        {
            if (!SelectedOpponentId.HasValue)
                return RedirectToPage();

            // Redirection GET pour conserver SelectedOpponentId dans l'URL
            return RedirectToPage(new { SelectedOpponentId });
        }

        public async Task<List<MatchDTO>> GetMatchesByOpponentAsync()
        {
            return await _context.TableTennisMatch
                .Where(m => m.OpponentId == SelectedOpponentId)

                // Compétition
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Competition)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Season)

                // Adversaire + clubs par saison
                .Include(m => m.Opponent)
                    .ThenInclude(p => p.PlayerSeasons)
                        .ThenInclude(pc => pc.Club)

                // Sets
                .Include(m => m.Sets)

                .OrderByDescending(m => m.Date_match)
                .ThenByDescending(m =>m.Id)

                .Select(m => new MatchDTO
                {
                    Id = m.Id,
                    Date_of_match = m.Date_match,

                    Competition = m.CompetitionCoefficient.Competition.Name,
                    Coefficient = m.CompetitionCoefficient.Coefficient,

                    Opponent_first_name = m.Opponent.First_name,
                    Opponent_last_name = m.Opponent.Last_name,

                    Opponent_club = m.Opponent.PlayerSeasons
                        .Where(pc => pc.SeasonId == m.CompetitionCoefficient.SeasonId)
                        .Select(pc => pc.Club.Name)
                        .FirstOrDefault(),

                    Season_name = m.CompetitionCoefficient.Season.Name,

                    My_points_at_match = m.My_points_at_match,
                    Opponent_points_at_match = m.Opponent_points_at_match,
                    Point_difference = m.Opponent_points_at_match - m.My_points_at_match,
                    Comment = m.Comment,

                    //Result = (MatchDTO.MatchResult)m.Result,
                    Result = m.Result == TableTennisMatch.MatchResult.V
                        ? MatchDTO.MatchResult.V
                        : m.Result == TableTennisMatch.MatchResult.D
                            ? MatchDTO.MatchResult.D
                            : MatchDTO.MatchResult.F,

                    MatchSets = m.Sets
                        .OrderBy(s => s.SetNumber)
                        .Select(s => new SetDTO
                        {
                            SetNumber = s.SetNumber,
                            Player1Score = s.Player1Score,
                            Player2Score = s.Player2Score
                        })
                        .ToList()
                })
                .ToListAsync();
        }
    }
}
