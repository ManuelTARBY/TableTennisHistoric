using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class ManagePlayerSeasonModel : PageModel
    {
        private readonly IPlayerService _playerService;

        public ManagePlayerSeasonModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [BindProperty(SupportsGet = true)]
        public int PlayerId { get; set; }

        public string PlayerFullName { get; set; } = "";

        // Propriété calculée
        public string DeOuD => string.IsNullOrEmpty(PlayerFullName)
            ? "de "
            : "aeiouàâäéèêëîïôöùûüæœAEIOUÀÂÄÉÈÊËÎÏÔÖÙÛÜÆŒ".Contains(PlayerFullName[0])
                ? "d'"
                : "de ";
        
        public List<PlayerSeasonDTO> PlayerSeasons { get; set; } = new();

        [BindProperty]
        public PlayerSeasonDTO EditForm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (PlayerId == 0)
                return RedirectToPage("/OpponentsHistoric");

            PlayerSeasons = await _playerService.GetPlayerSeasonsByPlayerIdAsync(PlayerId);

            if (PlayerSeasons.Any())
                PlayerFullName = PlayerSeasons.First().PlayerFullName;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid)
            {
                PlayerSeasons = await _playerService.GetPlayerSeasonsByPlayerIdAsync(PlayerId);
                return Page();
            }

            await _playerService.UpdatePlayerSeasonAsync(EditForm);
            return RedirectToPage(new { PlayerId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _playerService.DeletePlayerSeasonAsync(id);
            return RedirectToPage(new { PlayerId });
        }
    }
}