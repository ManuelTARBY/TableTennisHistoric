using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class AffiliateModel : PageModel
    {
        private readonly IPlayerService _playerService;

        public AffiliateModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [BindProperty(SupportsGet = true)]
        public int SelectedPlayerId { get; set; }

        [BindProperty]
        public int SelectedClubId { get; set; }

        [BindProperty]
        public int SelectedSeasonId { get; set; }

        [BindProperty]
        public decimal StartPoints { get; set; }

        [BindProperty]
        public decimal MiddlePoints { get; set; }

        [BindProperty]
        public PlayerSeason.PlayerCategory? SelectedCategory { get; set; }

        public SelectList Players { get; set; } = null!;
        public SelectList Clubs { get; set; } = null!;
        public SelectList Seasons { get; set; } = null!;

        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (SelectedPlayerId == 0 || SelectedClubId == 0 || SelectedSeasonId == 0)
            {
                ModelState.AddModelError(string.Empty, "Tous les champs sont obligatoires.");
                await LoadSelectListsAsync();
                return Page();
            }

            var (success, error) = await _playerService.CreatePlayerSeasonAsync(new PlayerSeason
            {
                PlayerId = SelectedPlayerId,
                SeasonId = SelectedSeasonId,
                ClubId = SelectedClubId,
                Category = SelectedCategory!.Value,
                Points_start = StartPoints,
                Points_middle = MiddlePoints
            });

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadSelectListsAsync();
                return Page();
            }

            SuccessMessage = "L'affiliation a été créée avec succès.";
            SelectedPlayerId = 0;
            SelectedClubId = 0;
            SelectedSeasonId = 0;
            SelectedCategory = null;
            StartPoints = 0;
            MiddlePoints = 0;
            ModelState.Clear();
            await LoadSelectListsAsync();
            return Page();
        }

        private async Task LoadSelectListsAsync()
        {
            var lists = await _playerService.GetCreatePlayerSelectListsAsync();
            Players = lists.Players;
            Clubs = lists.Clubs;
            Seasons = lists.Seasons;
        }
    }
}