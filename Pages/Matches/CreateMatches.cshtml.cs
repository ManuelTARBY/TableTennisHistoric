using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using TableTennisHistoric.Models;
using TableTennisHistoric.Datas;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Services;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Matches
{
    public class CreateMatchModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;
        private readonly ICompetitionService _competitionService;
        private readonly SeasonService _seasonService;

        public CreateMatchModel(
            TableTennisHistoricDbContext context, ICompetitionService competitionService, SeasonService seasonService)
        {
            _context = context;
            _competitionService = competitionService;
            _seasonService = seasonService;
        }

        // SelectLists
        public List<SelectListItem> CompetitionCoefficients { get; set; } = new();
        public List<SelectListItem> Opponents { get; set; } = new();

        // Form model
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public DateTime Date_match { get; set; } = DateTime.Today;

            [Required]
            public int CompetitionCoefficientId { get; set; }

            [Required]
            public int OpponentId { get; set; }

            [Range(0, 4000)]
            public decimal My_points_at_match { get; set; }

            [Range(0, 4000)]
            public decimal Opponent_points_at_match { get; set; }

            [Required]
            public TableTennisMatch.MatchResult Result { get; set; }
        }

        // GET
        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
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

            var match = new TableTennisMatch
            {
                Date_match = DateOnly.FromDateTime(Input.Date_match),
                CompetitionCoefficientId = Input.CompetitionCoefficientId,
                OpponentId = Input.OpponentId,
                My_points_at_match = Input.My_points_at_match,
                Opponent_points_at_match = Input.Opponent_points_at_match,
                Result = Input.Result,

                CompetitionCoefficient = competitionCoefficient,
                Opponent = opponent
            };

            _context.TableTennisMatch.Add(match);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private async Task LoadSelectListsAsync()
        {

            Season? currentSeason = await _seasonService.GetCurrentSeasonAsync();

            if (currentSeason == null) return ;

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
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .ToListAsync();

            Opponents = Opponents.OrderBy(o => o.Text).ToList();
        }
    }
}
