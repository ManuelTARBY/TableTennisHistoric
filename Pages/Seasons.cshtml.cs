using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class SeasonsModel : PageModel
    {
        private readonly ISeasonService _seasonService;

        public SeasonsModel(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        public List<SeasonEditDTO> Seasons { get; set; } = new();

        [BindProperty]
        public SeasonDTO CreateForm { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public SeasonEditDTO EditForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
            CreateForm = new SeasonDTO
            {
                Start_date = DateOnly.FromDateTime(DateTime.Now),
                End_date = DateOnly.FromDateTime(DateTime.Now),
                Phase1_End_date = DateOnly.FromDateTime(DateTime.Now)
            };
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {

            ModelState.Remove("CreateForm");

            if (!ModelState.IsValid)
            {
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            var (success, error) = await _seasonService.UpdateSeasonAsync(EditForm);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            // On ignore le formulaire du tableau
            ModelState.Remove("EditForm");

            // On vérifie si les données de création sont valides
            if (!ModelState.IsValid)
            {
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            // Appel au service avec votre DTO validé
            var (success, error) = await _seasonService.CreateSeasonAsync(CreateForm);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            // Préparation du message de succès
            SuccessMessage = $"La saison {CreateForm.Start_date:yyyy}-{CreateForm.End_date:yyyy} a bien été créée.";

            // Redirection (qui va reconstruire proprement un modèle tout neuf)
            return RedirectToPage();
        }
    }
}