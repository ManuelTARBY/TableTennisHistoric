using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Championship
{
    public class CreateModel : PageModel
    {
        private readonly IChampionshipService _championshipService;

        public CreateModel(IChampionshipService championshipService)
        {
            _championshipService = championshipService;
        }

        [BindProperty] public int SelectedChampionshipId { get; set; }
        [BindProperty] public int TeamCount { get; set; }
        [BindProperty] public List<TeamLineModel> TeamLines { get; set; } = new();
        [BindProperty] public int NbOfTeam { get; set; }
        [BindProperty] public int CSSeasonId { get; set; }
        [BindProperty] public int CSPhase { get; set; }
        [BindProperty] public int CSRank { get; set; }
        [BindProperty] public int CSLevel { get; set; }
        [BindProperty] public int CSGroup { get; set; }
        [BindProperty] public DateOnly? CSBeginning { get; set; }
        [BindProperty] public DateOnly? CSEnding { get; set; }

        public List<Models.Championship> Championships { get; set; } = new();
        public List<Club> Clubs { get; set; } = new();
        public SelectList SeasonSelectList { get; set; }
        public Dictionary<int, string> RankLabels => Models.Championship.RankLabels;

        public class TeamLineModel
        {
            public int Number { get; set; }
            public int TeamId { get; set; }
            public int ClubId { get; set; }
            public int ChampionshipId { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadBaseDataAsync();
        }

        public async Task<IActionResult> OnPostSelectTeamCountAsync()
        {
            await LoadBaseDataAsync();

            TeamLines = Enumerable.Range(1, TeamCount)
                .Select(i => new TeamLineModel { Number = i })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveTeams()
        {
            if (!ModelState.IsValid)
            {
                await LoadBaseDataAsync();
                return Page();
            }

            var teamLines = TeamLines.Select(tlm => new ChampionshipTeam
            {
                ChampionshipId = SelectedChampionshipId,
                TeamId = tlm.TeamId,
                Number = tlm.Number
            }).ToList();

            await _championshipService.SaveTeamsAndGenerateMatchesAsync(SelectedChampionshipId, teamLines);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateChampionship()
        {
            if (!ModelState.IsValid)
            {
                await LoadBaseDataAsync();
                return Page();
            }

            await _championshipService.CreateChampionshipAsync(new Models.Championship
            {
                SeasonId = CSSeasonId,
                Phase = CSPhase,
                Rank = CSRank,
                Level = CSLevel,
                ChampionshipGroup = CSGroup,
                Beginning = CSBeginning,
                Ending = CSEnding
            });

            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetTeamsByClubAsync(int clubId)
        {
            var teams = await _championshipService.GetTeamsByClubAsync(clubId);
            return new JsonResult(teams);
        }

        private async Task LoadBaseDataAsync()
        {
            var data = await _championshipService.GetCreatePageDataAsync();
            SeasonSelectList = data.SeasonSelectList;
            Championships = data.Championships;
            Clubs = data.Clubs;
        }
    }
}