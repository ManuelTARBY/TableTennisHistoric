using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services;
using TableTennisHistoric.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TableTennisHistoric.Pages.Players.CreateModel;

namespace TableTennisHistoric.Pages
{
    public class UpdateSeasonModel : PageModel
    {
        private readonly ISeasonService _seasonService;

        public UpdateSeasonModel(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        public List<SeasonEditDTO> Seasons { get; set; } = new();

        [BindProperty]
        public CreateSeasonForm CreateForm { get; set; } = new();
        [TempData]
        public string? SuccessMessage { get; set; }
        public class CreateSeasonForm
        {
            public DateOnly Start_Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
            public DateOnly End_Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
            public DateOnly Phase1_End_Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
            public decimal? p1_drift { get; set; }
            public decimal? p2_drift { get; set; }
        }

        [BindProperty]
        public SeasonEditDTO EditForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
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

            if (!ModelState.IsValid)
            {
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            ModelState.Clear();

            var(success, error) =  await _seasonService.CreateSeasonAsync(new Models.Season
                {
                    Name = $"Saison {CreateForm.Start_Date:yyyy}-{CreateForm.End_Date:yyyy}",
                    Start_date = CreateForm.Start_Date,
                    End_date = CreateForm.End_Date,
                    Phase1_End_date = CreateForm.Phase1_End_Date,
                    p1_drift = CreateForm.p1_drift,
                    p2_drift = CreateForm.p2_drift
                });


            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
                return Page();
            }

            // Réinitialisation des champs
            SuccessMessage = $"La saison {CreateForm.Start_Date:yyyy}-{CreateForm.End_Date:yyyy} a bien été créée.";
            CreateForm = new CreateSeasonForm();
            ModelState.Clear();

            Seasons = await _seasonService.GetAllSeasonEditDTOAsync();
            return RedirectToPage();
        }
    }
}