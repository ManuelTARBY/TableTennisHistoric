//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System.ComponentModel.DataAnnotations;
//using TableTennisHistoric.Datas;
//using TableTennisHistoric.Models;

//namespace TableTennisHistoric.Pages.Championship
//{
//    public class CreateModel : PageModel
//    {
//        private readonly TableTennisHistoricDbContext _context;

//        public CreateModel(TableTennisHistoricDbContext context)
//        {
//            _context = context;
//        }

//        [BindProperty]
//        public int SelectedChampionshipId { get; set; }
//        [BindProperty]
//        public int TeamCount { get; set; }

//        [BindProperty]
//        public List<TeamLineModel> TeamLines { get; set; } = new();

//        public List<Models.Championship> Championships { get; set; }
//        public List<Club> Clubs { get; set; }
//        public Dictionary<int, string> ConvertRank = Models.Championship.RankLabels;
//        public class TeamLineModel
//        {
//            public int Number { get; set; }
//            public int TeamId { get; set; }
//            public int ClubId { get; set; }
//            public int ChampionshipId { get; set; }
//        }
//        public SelectList ChampionshipSelectList { get; set; }
//        [BindProperty]
//        public int NbOfTeam { get; set; }
//        [BindProperty]
//        public int CSSeasonId { get; set; }
//        public SelectList SeasonSelectList { get; set; }
//        [BindProperty]
//        public int CSPhase { get; set; }
//        public SelectList PhaseSelectList { get; set; }
//        public Dictionary<int, string> RankLabels => Models.Championship.RankLabels;
//        [BindProperty]
//        public int CSRank { get; set; }
//        public SelectList RankSelectList { get; set; }
//        [BindProperty]
//        public int CSLevel { get; set; }
//        public SelectList LevelSelectList { get; set; }
//        [BindProperty]
//        public int CSGroup { get; set; }
//        [BindProperty]
//        public DateOnly? CSBeginning { get; set; }
//        [BindProperty]
//        public DateOnly? CSEnding { get; set; }
//        public SelectList GroupSelectList { get; set; }

//        public async Task OnGetAsync()
//        {
//            await LoadBaseData();
//        }

//        private async Task LoadBaseData()
//        {
//            var Seasons = await _context.Season.OrderByDescending(c => c.Name).ToListAsync();
//            SeasonSelectList = new SelectList(Seasons, "Id", "Name");

//            Championships = await _context.Championship.OrderByDescending(c => c.Beginning).ToListAsync();
//            Clubs = await _context.Club
//                .OrderBy(c => c.Name)
//                .ToListAsync();
//        }

//        public async Task<IActionResult> OnPostSelectTeamCountAsync()
//        {
//            await LoadBaseData();

//            TeamLines = Enumerable.Range(1, TeamCount)
//                .Select(i => new TeamLineModel { Number = i })
//                .ToList();

//            return Page();
//        }

//        public async Task<IActionResult> OnPostSaveTeams()
//        {
//            if (!ModelState.IsValid)
//            {
//                await LoadBaseData();
//                return Page();
//            }

//            // Création des ChampionshipTeam 
//            var championshipTeams = TeamLines.Select(tlm => new ChampionshipTeam
//            {
//                ChampionshipId = SelectedChampionshipId,
//                TeamId = tlm.TeamId,
//                Number = tlm.Number
//            }).ToList();

//            _context.ChampionshipTeam.AddRange(championshipTeams);
//            await _context.SaveChangesAsync();

//            // Création des ChampionshipMatch
//            int Nb_of_Team = championshipTeams.Count % 2 == 0 ? championshipTeams.Count : championshipTeams.Count + 1;

//            // On récupère les formats
//            List<ChampionshipFormat> CsFormats =
//                await _context.ChampionshipFormat
//                    .Where(cf => cf.Nb_of_team == Nb_of_Team)
//                    .OrderBy(cf => cf.Matchday)
//                    .ToListAsync();

//            // On crée les ChampionshipMatch
//            List<ChampionshipMatch> CsMatches = new List<ChampionshipMatch>();

//            foreach (var CsFormat in CsFormats)
//            {
//                string[] HomeList = CsFormat.Home.Split(',');
//                string[] AwayList = CsFormat.Away.Split(',');

//                for (int i = 0; i < HomeList.Length; i++)
//                {
//                    // Convertit correctement "1", "2", etc.
//                    int homeNumber = int.Parse(HomeList[i]);
//                    int awayNumber = int.Parse(AwayList[i]);

//                    // On récupère le ChampionshipTeam correspondant
//                    var homeTeam = championshipTeams.First(ct => ct.Number == homeNumber);
//                    var awayTeam = championshipTeams.First(ct => ct.Number == awayNumber);

//                    CsMatches.Add(new ChampionshipMatch
//                    {
//                        ChampionshipId = SelectedChampionshipId,
//                        Matchday = CsFormat.Matchday,
//                        Home_teamId = homeTeam.Id,     // ID du ChampionshipTeam
//                        Away_teamId = awayTeam.Id
//                    });
//                }
//            }

//            // Enregistrement en base
//            _context.ChampionshipMatch.AddRange(CsMatches);
//            await _context.SaveChangesAsync();

//            return RedirectToPage();
//        }

//        public async Task<IActionResult> OnPostCreateChampionship()
//        {
//            if (!ModelState.IsValid)
//            {
//                await LoadBaseData();
//                return Page();
//            }

//            var championship = new Models.Championship
//            {
//                SeasonId = CSSeasonId,
//                Phase = CSPhase,
//                Rank = CSRank,
//                Level = CSLevel,
//                ChampionshipGroup = CSGroup,
//                Beginning = CSBeginning,
//                Ending = CSEnding
//            };

//            _context.Championship.Add(championship);
//            await _context.SaveChangesAsync();

//            return RedirectToPage();
//        }

//        public JsonResult OnGetTeamsByClub(int clubId)
//        {
//            var teams = _context.Team
//                .Where(t => t.ClubId == clubId)
//                .Select(t => new { id = t.Id, name = t.Name })
//                .ToList();

//            return new JsonResult(teams);
//        }
//    }
//}

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