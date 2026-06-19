using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class CreateModel : PageModel
    {
        private readonly IPlayerService _playerService;

        public CreateModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public List<PlayerEditDTO> Players { get; set; } = new();

        [BindProperty]
        public CreatePlayerForm CreateForm { get; set; } = new();

        [BindProperty]
        public PlayerEditDTO EditForm { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public class CreatePlayerForm
        {
            public string First_name { get; set; } = "";
            public string Last_name { get; set; } = "";
            public string? License_number { get; set; }
        }

        public async Task OnGetAsync()
        {
            Players = await _playerService.GetAllPlayerEditDTOAsync();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(CreateForm.First_name) || string.IsNullOrWhiteSpace(CreateForm.Last_name))
            {
                ModelState.AddModelError(string.Empty, "Le prénom et le nom sont obligatoires.");
                Players = await _playerService.GetAllPlayerEditDTOAsync();
                return Page();
            }

            if (await _playerService.GetPlayerByLicenseNumberAsync(CreateForm.License_number) != null)
            {
                ModelState.AddModelError(string.Empty, "Un joueur est déjà enregistré sous ce numéro de license.");
                Players = await _playerService.GetAllPlayerEditDTOAsync();
                return Page();
            }

            await _playerService.CreatePlayerAsync(new Models.Player
            {
                First_name = CreateForm.First_name,
                Last_name = CreateForm.Last_name,
                License_number = CreateForm.License_number
            });

            SuccessMessage = $"Le joueur {CreateForm.First_name} {CreateForm.Last_name.ToUpper()} a bien été créé.";

            // Réinitialisation des champs
            CreateForm = new CreatePlayerForm();
            ModelState.Clear();

            Players = await _playerService.GetAllPlayerEditDTOAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(EditForm.First_name) || string.IsNullOrWhiteSpace(EditForm.Last_name))
            {
                ModelState.AddModelError(string.Empty, "Le prénom et le nom sont obligatoires.");
                Players = await _playerService.GetAllPlayerEditDTOAsync();
                return Page();
            }

            var (success, error) = await _playerService.UpdatePlayerAsync(EditForm);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error!);
                Players = await _playerService.GetAllPlayerEditDTOAsync();
                return Page();
            }

            Players = await _playerService.GetAllPlayerEditDTOAsync();
            return Page();
        }
    }
}