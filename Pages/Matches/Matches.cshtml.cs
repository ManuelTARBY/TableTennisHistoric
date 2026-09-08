using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class MatchesModel : PageModel
    {
        private readonly IMatchService _matchService;
        private readonly ISeasonService _seasonService;
        private readonly ICompetitionService _competitionService;

        public MatchesModel(IMatchService matchService, ISeasonService seasonService, ICompetitionService competitionService)
        {
            _matchService = matchService;
            _seasonService = seasonService;
            _competitionService = competitionService;
        }

        public List<MatchDTO> MatchesDTO { get; set; } = new();
        public IEnumerable<SelectListItem> CompetitionCoefficients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Competitions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CompetitionSupplements { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Stages { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Opponents { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public DateTime Date_match { get; set; } = DateTime.Today;

            [Required]
            public int CompetitionCoefficientId { get; set; }
            [Required]
            public int CompetitionId { get; set; }
            public int? CompetitionSupplementId { get; set; } = null;
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
            // Restaure la valeur depuis la session
            var sessionValue = HttpContext.Session.GetString("MyLastPoints");
            if (sessionValue != null && decimal.TryParse(sessionValue,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal lastPoints))
            {
                Input.My_points_at_match = lastPoints;
            }

            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var season = await _seasonService.GetSeasonByDateAsync(DateOnly.FromDateTime(Input.Date_match));

            if (season == null)
            {
                ModelState.AddModelError("Input.Date_match", "Aucune saison n'existe pour la date saisie.");
                await LoadSelectListsAsync();
                return Page();
            }

            var coefficientCompetition = await _competitionService.GetCompetitionCoefficientByCompetitionAndSeasonAsync(Input.CompetitionId, season.Id);

            if (coefficientCompetition == null)
            {
                CompetitionCoefficient newCoefficientCompetition = new()
                {
                    CompetitionId = Input.CompetitionId,
                    SeasonId = season.Id,
                    Coefficient = 0.0m // Par défaut
                };
                await _competitionService.CreateCompetitionCoefficientAsync(newCoefficientCompetition);

                coefficientCompetition = newCoefficientCompetition;
            }

            var success = await _matchService.CreateMatchWithSetsAsync(new CreateMatchDTO
            {
                Date_match = Input.Date_match,
                CompetitionCoefficientId = coefficientCompetition.Id,
                CompetitionSupplementId = Input.CompetitionSupplementId,
                StageId = Input.StageId,
                OpponentId = Input.OpponentId,
                My_points_at_match = Input.My_points_at_match,
                Opponent_points_at_match = Input.Opponent_points_at_match,
                Result = Input.Result ?? TableTennisMatch.MatchResult.F,
                Comment = Input.Comment != "" ? Input.Comment : null,
                SetMy = Input.SetMy,
                SetOpp = Input.SetOpp
            });

            if (!success)
            {
                ModelState.AddModelError("", "Données invalides.");
                await LoadSelectListsAsync();
                return Page();
            }

            // Conserve la valeur en session
            HttpContext.Session.SetString("MyLastPoints",
                Input.My_points_at_match.ToString(System.Globalization.CultureInfo.InvariantCulture));

            return RedirectToPage();
        }

        private async Task LoadSelectListsAsync()
        {
            var data = await _matchService.GetMatchesPageDataAsync();
            MatchesDTO = data.MatchesDTO;
            CompetitionCoefficients = data.CompetitionCoefficients;
            Competitions = data.Competitions;
            CompetitionSupplements = data.CompetitionSupplements;
            Opponents = data.Opponents;
            Stages = data.Stages;
        }
    }
}