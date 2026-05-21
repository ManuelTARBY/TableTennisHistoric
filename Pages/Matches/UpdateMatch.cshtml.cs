using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Matches
{
    public class UpdateMatchModel : PageModel
    {
        private readonly IMatchService _matchService;

        public UpdateMatchModel(IMatchService matchService)
        {
            _matchService = matchService;
        }

        public List<SelectListItem> CompetitionCoefficients { get; set; } = new();
        public List<SelectListItem> Opponents { get; set; } = new();
        public IEnumerable<SelectListItem> Stages { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            public DateTime Date_match { get; set; }

            [Required]
            public int CompetitionCoefficientId { get; set; }

            [Required]
            public int OpponentId { get; set; }
            public int? StageId { get; set; } = null;

            public decimal My_points_at_match { get; set; }
            public decimal Opponent_points_at_match { get; set; }

            [Required]
            public TableTennisMatch.MatchResult Result { get; set; }

            public string? Comment { get; set; }

            public List<SetInputModel> Sets { get; set; } = new();
        }

        public class SetInputModel
        {
            public int SetNumber { get; set; }
            public int Player1Score { get; set; }
            public int Player2Score { get; set; }
        }

        // GET
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var match = await _matchService.GetMatchWithSetsAsync(id);

            if (match == null)
                return NotFound();

            var lists = await _matchService.GetUpdateMatchSelectListsAsync(match);
            CompetitionCoefficients = lists.CompetitionCoefficients;
            Opponents = lists.Opponents;
            Stages = lists.Stages;

            Input = new InputModel
            {
                Id = match.Id,
                Date_match = match.Date_match.ToDateTime(TimeOnly.MinValue),
                CompetitionCoefficientId = match.CompetitionCoefficientId,
                StageId = match.StageId,
                OpponentId = match.OpponentId,
                My_points_at_match = match.My_points_at_match,
                Opponent_points_at_match = match.Opponent_points_at_match,
                Result = match.Result,
                Comment = match.Comment,
                Sets = match.Sets?
                    .OrderBy(s => s.SetNumber)
                    .Select(s => new SetInputModel
                    {
                        SetNumber = s.SetNumber,
                        Player1Score = s.Player1Score,
                        Player2Score = s.Player2Score
                    }).ToList() ?? new List<SetInputModel>()
            };

            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var match = await _matchService.GetMatchWithSetsAsync(Input.Id);

            if (match == null)
                return NotFound();

            var lists = await _matchService.GetUpdateMatchSelectListsAsync(match);
            CompetitionCoefficients = lists.CompetitionCoefficients;
            Opponents = lists.Opponents;
            Stages = lists.Stages;

            // Filtrage des sets vides
            var filteredSets = Input.Sets
                .Where(s => (s.Player1Score > 0 || s.Player2Score > 0) && (s.Player1Score >= 11 || s.Player2Score >= 11))
                .Select(s => new MatchSet
                {
                    SetNumber = s.SetNumber,
                    Player1Score = s.Player1Score,
                    Player2Score = s.Player2Score
                }).ToList();

            // Validation des sets
            if (filteredSets.Count >= 3)
            {
                var setErrors = _matchService.ValidateSets(filteredSets, Input.Result);
                foreach (var error in setErrors)
                    ModelState.AddModelError(string.Empty, error);

                if (!ModelState.IsValid)
                    return Page();
            }

            await _matchService.UpdateMatchAsync(match, Input.CompetitionCoefficientId, Input.StageId,
                Input.OpponentId, Input.Date_match, Input.My_points_at_match,
                Input.Opponent_points_at_match, Input.Result, Input.Comment,
                filteredSets);

            return RedirectToPage("/Index");
        }
    }
}