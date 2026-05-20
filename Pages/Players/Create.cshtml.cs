using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;
using Microsoft.EntityFrameworkCore;

namespace TableTennisHistoric.Pages.Players
{
    public class CreateModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;

        public CreateModel(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        // --- JOUEUR ---
        [BindProperty]
        public Player Player { get; set; } = new();

        // --- PLAYERCLUB ---
        [BindProperty]
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
            Players = new SelectList(await _context.Player
                .OrderBy(p => p.First_name)
                .Select(p => new
                {
                    p.Id,
                    FullName = p.First_name + " " + p.Last_name
                })
                .ToListAsync(), "Id", "FullName");

            Clubs = new SelectList(await _context.Club
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name");
            Seasons = new SelectList(await _context.Season.ToListAsync(), "Id", "Name");
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

            Player.Last_name = Player.Last_name.ToUpper();
            _context.Player.Add(Player);
            await _context.SaveChangesAsync();

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

            var playerSeason = new PlayerSeason
            {
                PlayerId = SelectedPlayerId,
                SeasonId = SelectedSeasonId,
                ClubId = SelectedClubId,
                Category = SelectedCategory.Value,
                Points_start = StartPoints,
                Points_middle = MiddlePoints
            };

            _context.PlayerSeason.Add(playerSeason);

            await _context.SaveChangesAsync();

            SuccessMessagePlayerClub = "L'affiliation joueur/club/saison a été créée avec succès.";
            SelectedPlayerId = 0;
            SelectedClubId = 0;
            SelectedSeasonId = 0;
            SelectedCategory = null;
            StartPoints = 0;
            MiddlePoints = 0;
            await LoadSelectListsAsync();

            ModelState.Clear();
            return Page();
        }
    }
}
