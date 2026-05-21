using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Matches
{
    public class CreateMatchModel : PageModel
    {
        private readonly IMatchService _matchService;

        public CreateMatchModel(IMatchService matchService)
        {
            _matchService = matchService;
        }

        public List<SelectListItem> CompetitionCoefficients { get; set; } = new();
        public List<SelectListItem> Opponents { get; set; } = new();

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

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var success = await _matchService.CreateMatchAsync(Input.CompetitionCoefficientId, Input.OpponentId, new TableTennisMatch
            {
                Date_match = DateOnly.FromDateTime(Input.Date_match),
                CompetitionCoefficientId = Input.CompetitionCoefficientId,
                OpponentId = Input.OpponentId,
                My_points_at_match = Input.My_points_at_match,
                Opponent_points_at_match = Input.Opponent_points_at_match,
                Result = Input.Result
            });

            if (!success)
            {
                ModelState.AddModelError("", "Données invalides.");
                await LoadSelectListsAsync();
                return Page();
            }

            return RedirectToPage("/Index");
        }

        private async Task LoadSelectListsAsync()
        {
            var lists = await _matchService.GetCreateMatchSelectListsAsync();
            CompetitionCoefficients = lists.CompetitionCoefficients;
            Opponents = lists.Opponents;
        }
    }
}