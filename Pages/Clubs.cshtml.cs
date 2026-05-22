using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class ClubsModel : PageModel
    {
        private readonly IClubService _clubService;

        public ClubsModel(IClubService clubService)
        {
            _clubService = clubService;
        }

        public List<Club> Clubs { get; set; } = new();
        public SelectList ClubSelectList { get; set; }
        public List<Team> SelectedClubTeams { get; set; } = new();

        [BindProperty]
        public CreateClubModel CreateClubForm { get; set; } = new();

        [BindProperty]
        public int? SelectedClubId { get; set; }

        [BindProperty]
        public CreateTeamsModel CreateTeamsForm { get; set; } = new();

        public class CreateClubModel
        {
            [Required, StringLength(100)]
            public string Name { get; set; }

            [StringLength(100)]
            public string Name_abrev { get; set; }

            [StringLength(10)]
            public string License_number { get; set; }

            [StringLength(100)]
            public string City { get; set; }

            [Required, Range(1, 99)]
            public int Department { get; set; }
        }

        public class CreateTeamsModel
        {
            [Required]
            public int TeamClubId { get; set; }

            [Required, StringLength(100)]
            public string TeamBaseName { get; set; }

            [Required, Range(1, 100)]
            public int TeamsCount { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadBaseDataAsync();
        }

        public async Task<IActionResult> OnPostCreateClubAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(CreateClubForm))
            {
                await LoadBaseDataAsync();
                return Page();
            }

            await _clubService.CreateClubAsync(new Club
            {
                Name = CreateClubForm.Name,
                Name_abrev = CreateClubForm.Name_abrev,
                License_number = CreateClubForm.License_number,
                City = CreateClubForm.City,
                Department = CreateClubForm.Department
            });

            TempData["Success"] = "Club créé !";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSelectClubAsync()
        {
            ModelState.Clear();
            await LoadBaseDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateTeamsAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(CreateTeamsForm))
            {
                await LoadBaseDataAsync();
                return Page();
            }

            await _clubService.CreateTeamsAsync(CreateTeamsForm.TeamClubId, CreateTeamsForm.TeamBaseName, CreateTeamsForm.TeamsCount);

            SelectedClubId = CreateTeamsForm.TeamClubId;
            CreateTeamsForm = new CreateTeamsModel();

            await LoadBaseDataAsync();

            TempData["Success"] = "Équipes créées !";
            return Page();
        }

        private async Task LoadBaseDataAsync()
        {
            var data = await _clubService.GetClubsPageDataAsync(SelectedClubId);
            Clubs = data.Clubs;
            ClubSelectList = data.ClubSelectList;
            SelectedClubTeams = data.SelectedClubTeams;
        }
    }
}