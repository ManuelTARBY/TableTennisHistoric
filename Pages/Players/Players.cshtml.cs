using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class PlayersModel : PageModel
    {
        private readonly IPlayerService _playerService;

        public PlayersModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public List<PlayerEditDTO> Players { get; set; } = new();
        public SelectList Clubs { get; set; } = null!;
        public SelectList Seasons { get; set; } = null!;
        public string? SuccessMessage { get; set; }

        [BindProperty]
        public CreatePlayerWithAffiliationForm Form { get; set; } = new();

        public class CreatePlayerWithAffiliationForm
        {
            [Required(ErrorMessage = "Le prénom est obligatoire.")]
            public string First_name { get; set; } = "";

            [Required(ErrorMessage = "Le nom est obligatoire.")]
            public string Last_name { get; set; } = "";

            public string? License_number { get; set; }

            [Required(ErrorMessage = "La saison est obligatoire.")]
            public int SeasonId { get; set; }

            [Required(ErrorMessage = "Le club est obligatoire.")]
            public int ClubId { get; set; }

            [Required(ErrorMessage = "La catégorie est obligatoire.")]
            public PlayerSeason.PlayerCategory Category { get; set; }
            public decimal Points_start { get; set; }
            public decimal Points_middle { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var (success, error, playerId) = await _playerService.CreatePlayerWithAffiliationAsync(
                new Models.Player
                {
                    First_name = Form.First_name,
                    Last_name = Form.Last_name,
                    License_number = Form.License_number
                },
                new PlayerSeason
                {
                    SeasonId = Form.SeasonId,
                    ClubId = Form.ClubId,
                    Category = Form.Category,
                    Points_start = Form.Points_start,
                    Points_middle = Form.Points_middle
                }
            );

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadDataAsync();
                return Page();
            }

            SuccessMessage = $"Le joueur {Form.First_name} {Form.Last_name.ToUpper()} a bien été créé avec son affiliation.";
            ModelState.Clear();
            Form = new CreatePlayerWithAffiliationForm();
            Players = await _playerService.GetAllPlayerEditDTOAsync();
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(EditForm.First_name) || string.IsNullOrWhiteSpace(EditForm.Last_name))
            {
                ModelState.AddModelError(string.Empty, "Le prénom et le nom sont obligatoires.");
                await LoadDataAsync();
                return Page();
            }

            var (success, error) = await _playerService.UpdatePlayerAsync(EditForm);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                await LoadDataAsync();
                return Page();
            }

            await LoadDataAsync();
            return Page();
        }

        [BindProperty]
        public PlayerEditDTO EditForm { get; set; } = new();

        private async Task LoadDataAsync()
        {
            Players = await _playerService.GetAllPlayerEditDTOAsync();
            var lists = await _playerService.GetCreatePlayerSelectListsAsync();
            Clubs = lists.Clubs;
            Seasons = lists.Seasons;
        }
    }
}