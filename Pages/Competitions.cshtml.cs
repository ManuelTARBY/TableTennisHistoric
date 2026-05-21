using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class CompetitionsModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;
        private readonly ISeasonService _seasonService;
        private readonly ICompetitionService _competitionService;

        public CompetitionsModel(TableTennisHistoricDbContext context, ISeasonService seasonService, ICompetitionService competitionService)
        {
            _context = context;
            _seasonService = seasonService;
            _competitionService = competitionService;
        }

        public Season? CurrentSeason;
        public List<Season>? AllSeason { get; set; } = new();
        public List<SeasonCompetitionDTO>? CompetitionWithCoefficient { get; set; } = new();
        public List<Competition>? Competition { get; set; } = new();
        [BindProperty(SupportsGet = false)]
        public Competition NewCompetition { get; set; } = new();
        [BindProperty(SupportsGet = false)]
        public CompetitionCoefficient NewCompetitionCoefficient { get; set; } = new();
        public SelectList SeasonList { get; set; } = null!;
        public SelectList CompetitionList { get; set; } = null!;

        [TempData]
        public string? StatusMessage { get; set; }


        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();
        }

        public async Task LoadSelectListsAsync()
        {
            // Saison en cours
            CurrentSeason = await _seasonService.GetCurrentSeasonAsync();

            // Toutes les saisons
            AllSeason = await _seasonService.GetAllSeasonsAsync();

            // Compétitions avec coefficient pour la saison en cours
            Competition = await _competitionService.GetAllCompetitionAsync();

            // Compétitions avec coefficient pour la saison en cours
            if (CurrentSeason is not null)
            {
                CompetitionWithCoefficient =
                    await _competitionService.GetCompetitionDTOBySeasonAsync(CurrentSeason.Id);
            }
            else
            {
                CompetitionWithCoefficient = new();
            }

            SeasonList = new SelectList(await _context.Season
                .OrderByDescending(s => s.Start_date)
                .Select(s => new
                {
                    s.Id,
                    s.Name
                })
                .ToListAsync(), "Id", "Name");

            CompetitionList = new SelectList(await _context.Competition
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync(), "Id", "Name");

        }

        public async Task<IActionResult> OnPostCreateCompetitionAsync()
        {
            // 1. Nettoyer TOUTES les erreurs pour repartir à zéro
            ModelState.Clear();

            // 2. Valider manuellement uniquement le champ nécessaire
            if (string.IsNullOrWhiteSpace(NewCompetition.Name))
            {
                ModelState.AddModelError("NewCompetition.Name", "Le nom de la compétition est requis.");
            }

            // 3. Si on a des erreurs (ou si le TryValidate spécifique échoue)
            if (!ModelState.IsValid)
            {
                // TRÈS IMPORTANT : Recharger les listes avant de retourner la Page
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                _context.Competition.Add(NewCompetition);
                await _context.SaveChangesAsync();
                StatusMessage = "La compétition a été créée avec succès.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de la sauvegarde : " + ex.Message);
                await LoadSelectListsAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateCompetitionCoefficientAsync()
        {
            // 1. On vide les erreurs automatiques (notamment celles de NewCompetition.Name)
            ModelState.Clear();

            // 2. Validation manuelle des champs critiques
            if (NewCompetitionCoefficient.CompetitionId <= 0)
            {
                ModelState.AddModelError("NewCompetitionCoefficient.CompetitionId", "Veuillez choisir une compétition.");
            }

            if (NewCompetitionCoefficient.SeasonId <= 0)
            {
                ModelState.AddModelError("NewCompetitionCoefficient.SeasonId", "Veuillez choisir une saison.");
            }

            // 3. Si des erreurs existent, on recharge et on reste sur la page
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                _context.CompetitionCoefficient.Add(NewCompetitionCoefficient);
                await _context.SaveChangesAsync();

                StatusMessage = "Le coefficient a été attribué avec succès.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de la sauvegarde : " + ex.Message);
                await LoadSelectListsAsync();
                return Page();
            }
        }
    }
}
