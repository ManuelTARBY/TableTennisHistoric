using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class ManagePlayerSeasonModel : PageModel
    {
        private readonly IPlayerService _playerService;
        private readonly IClubService _clubService;

        public ManagePlayerSeasonModel(IPlayerService playerService, IClubService clubService)
        {
            _playerService = playerService;
            _clubService = clubService;
        }

        [BindProperty(SupportsGet = true)]
        public int PlayerId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string PlayerFullName { get; set; } = "";

        // Propriété calculée
        public string DeOuD => string.IsNullOrEmpty(PlayerFullName)
            ? "de "
            : "aeiouàâäéèêëîïôöùûüæœAEIOUÀÂÄÉÈÊËÎÏÔÖÙÛÜÆŒ".Contains(PlayerFullName[0])
                ? "d'"
                : "de ";
        
        public List<PlayerSeasonDTO> PlayerSeasons { get; set; } = new();
        public SelectList Clubs { get; set; } = null!;

        [BindProperty]
        public PlayerSeasonDTO EditForm { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (PlayerId == 0)
                return RedirectToPage("/OpponentsHistoric");

            PlayerSeasons = await _playerService.GetPlayerSeasonsByPlayerIdAsync(PlayerId);

            Clubs = await _clubService.GetClubsSelectListAsync();

            if (PlayerSeasons.Any())
                PlayerFullName = PlayerSeasons.First().PlayerFullName;

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {

            var clubId = EditForm.ClubId;

            if (!ModelState.IsValid)
            {
                PlayerSeasons = await _playerService.GetPlayerSeasonsByPlayerIdAsync(PlayerId);
                Clubs = await _clubService.GetClubsSelectListAsync();
                return Page();
            }

            await _playerService.UpdatePlayerSeasonAsync(EditForm);
            return RedirectToPage(new { PlayerId, ReturnUrl });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _playerService.DeletePlayerSeasonAsync(id);
            return RedirectToPage(new { PlayerId, ReturnUrl });
        }
    }
}