using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Pages
{
    public class ClubsModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;

        public ClubsModel(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        // ------------------------------
        //   LISTE DES CLUBS
        // ------------------------------
        public List<Club> Clubs { get; set; } = new();

        // Formulaire Création de club
        [BindProperty]
        public CreateClubModel CreateClubForm { get; set; } = new();

        // ------------------------------
        //   SECTION ÉQUIPES
        // ------------------------------

        public SelectList ClubSelectList { get; set; }

        // Sélection d’un club pour afficher ses équipes
        [BindProperty]
        public int? SelectedClubId { get; set; }
        public List<Team> SelectedClubTeams { get; set; }

        // Formulaire Création d’équipes
        [BindProperty]
        public CreateTeamsModel CreateTeamsForm { get; set; } = new();

        // ------------------------------
        //   GET
        // ------------------------------
        public async Task OnGetAsync()
        {
            await LoadBaseData();
        }

        private async Task LoadBaseData()
        {
            Clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            ClubSelectList = new SelectList(Clubs, "Id", "Name");

            if (SelectedClubId.HasValue)
            {
                SelectedClubTeams = await _context.Team
                    .Where(t => t.ClubId == SelectedClubId.Value)
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }
        }

        // ------------------------------
        //   POST : Ajout d'un club
        // ------------------------------
        public async Task<IActionResult> OnPostCreateClubAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(CreateClubForm))
            {
                await LoadBaseData();
                return Page();
            }

            var newClub = new Club
            {
                Name = CreateClubForm.Name,
                Name_abrev = CreateClubForm.Name_abrev,
                License_number = CreateClubForm.License_number,
                City = CreateClubForm.City,
                Department = CreateClubForm.Department
            };

            _context.Club.Add(newClub);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Club créé !";
            return RedirectToPage();
        }

        // ------------------------------
        //   POST : Sélection d'un club pour afficher ses équipes
        // ------------------------------
        public async Task<IActionResult> OnPostSelectClubAsync()
        {
            ModelState.Clear();
            await LoadBaseData();
            return Page();
        }

        // ------------------------------
        //   POST : Création de plusieurs équipes
        // ------------------------------
        public async Task<IActionResult> OnPostCreateTeamsAsync()
        {
            ModelState.Clear();

            if (!TryValidateModel(CreateTeamsForm))
            {
                await LoadBaseData();
                return Page();
            }

            for (int i = 1; i <= CreateTeamsForm.TeamsCount; i++)
            {
                var team = new Team
                {
                    Name = $"{CreateTeamsForm.TeamBaseName} {i}",
                    ClubId = CreateTeamsForm.TeamClubId
                };
                _context.Team.Add(team);
            }

            await _context.SaveChangesAsync();

            SelectedClubId = CreateTeamsForm.TeamClubId;

            CreateTeamsForm = new CreateTeamsModel();

            await LoadBaseData();

            TempData["Success"] = "Équipes créées !";
            return Page();
        }

        // ------------------------------
        //   VIEWMODELS
        // ------------------------------

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

            [Required]
            [Range(1, 99)]
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
    }
}
