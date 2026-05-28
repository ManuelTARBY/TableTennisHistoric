using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class CreateModel : PageModel
    {
        public IPlayerService _playerService { get; }

        public CreateModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        // --- JOUEUR ---
        [BindProperty]
        public Player Player { get; set; } = new();

        // --- PLAYERCLUB ---
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

        // --- SELECT LISTS ---
        public SelectList Players { get; set; } = null!;
        public SelectList Clubs { get; set; } = null!;
        public SelectList Seasons { get; set; } = null!;

        // --- MESSAGES ---
        public string? SuccessMessagePlayer { get; set; }
        public string? SuccessMessagePlayerClub { get; set; }

        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        private async Task LoadSelectListsAsync()
        {
            var lists = await _playerService.GetCreatePlayerSelectListsAsync();
            Players = lists.Players;
            Clubs = lists.Clubs;
            Seasons = lists.Seasons;
        }

        // ==============================
        //   FORMULAIRE 1 : CRÉER JOUEUR
        // ==============================
        public async Task<IActionResult> OnPostCreatePlayerAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            await _playerService.CreatePlayerAsync(Player);

            SuccessMessagePlayer = $"Le joueur {Player.First_name} {Player.Last_name.ToUpper()} a bien été créé.";
            await LoadSelectListsAsync();
            ModelState.Clear();
            Player = new Player();

            return Page();
        }

        // ==============================
        //   FORMULAIRE 2 : CRÉER PLAYERSEASON
        // ==============================
        public async Task<IActionResult> OnPostCreatePlayerSeasonAsync()
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

            SuccessMessagePlayerClub = "L'affiliation joueur/club/saison a été créée avec succès.";
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
    }
}
