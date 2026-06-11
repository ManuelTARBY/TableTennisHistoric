using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class CompetitionsModel : PageModel
    {
        private readonly ICompetitionService _competitionService;

        public CompetitionsModel(ICompetitionService competitionService)
        {
            _competitionService = competitionService;
        }

        public Season? CurrentSeason;
        public List<Season>? AllSeason { get; set; } = new();
        public List<SeasonCompetitionDTO>? CompetitionWithCoefficient { get; set; } = new();
        public List<Competition>? Competition { get; set; } = new();
        public SelectList SeasonList { get; set; } = null!;
        public SelectList CompetitionList { get; set; } = null!;

        [BindProperty(SupportsGet = false)]
        public Competition NewCompetition { get; set; } = new();

        [BindProperty(SupportsGet = false)]
        public CompetitionCoefficient NewCompetitionCoefficient { get; set; } = new();
        [BindProperty]
        public CoefficientCompetitionEditDTO EditCoefficientForm { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostCreateCompetitionAsync()
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(NewCompetition.Name))
            {
                ModelState.AddModelError("NewCompetition.Name", "Le nom de la compétition est requis.");
                await LoadSelectListsAsync();
                return Page();
            }

            var (success, error) = await _competitionService.CreateCompetitionAsync(NewCompetition);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadSelectListsAsync();
                return Page();
            }

            StatusMessage = "La compétition a été créée avec succès.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateCompetitionCoefficientAsync()
        {
            ModelState.Clear();

            if (NewCompetitionCoefficient.CompetitionId <= 0)
                ModelState.AddModelError("NewCompetitionCoefficient.CompetitionId", "Veuillez choisir une compétition.");

            if (NewCompetitionCoefficient.SeasonId <= 0)
                ModelState.AddModelError("NewCompetitionCoefficient.SeasonId", "Veuillez choisir une saison.");

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            var (success, error) = await _competitionService.CreateCompetitionCoefficientAsync(NewCompetitionCoefficient);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadSelectListsAsync();
                return Page();
            }

            StatusMessage = "Le coefficient a été attribué avec succès.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateCoefficientAsync()
        {
            ModelState.Clear(); // ← supprime TOUTES les erreurs de validation

            // Revalide uniquement EditCoefficientForm
            if (EditCoefficientForm.Id <= 0)
            {
                ModelState.AddModelError(string.Empty, "Identifiant du coefficient invalide.");
                await LoadSelectListsAsync();
                return Page();
            }

            if (EditCoefficientForm.Coefficient <= 0)
            {
                ModelState.AddModelError(string.Empty, "Le coefficient doit être supérieur à 0.");
                await LoadSelectListsAsync();
                return Page();
            }

            var (success, error) = await _competitionService.UpdateCoefficientCompetitionAsync(EditCoefficientForm);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadSelectListsAsync();
                return Page();
            }

            StatusMessage = "Coefficient modifié !";
            return RedirectToPage();
        }

        private async Task LoadSelectListsAsync()
        {
            var data = await _competitionService.GetCompetitionsPageDataAsync();
            CurrentSeason = data.CurrentSeason;
            AllSeason = data.AllSeasons;
            Competition = data.Competitions;
            CompetitionWithCoefficient = data.CompetitionWithCoefficient;
            SeasonList = data.SeasonList;
            CompetitionList = data.CompetitionList;
        }
    }
}