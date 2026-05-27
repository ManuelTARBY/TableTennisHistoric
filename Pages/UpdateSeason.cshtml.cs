using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

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
    }
}