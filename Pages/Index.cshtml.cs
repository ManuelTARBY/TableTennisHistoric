using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IIndexService _indexService;
        private readonly ISeasonService _seasonService;
        private readonly IMatchService _matchService;

        public IndexModel(IIndexService indexService, ISeasonService seasonService, IMatchService matchService)
        {
            _indexService = indexService;
            _seasonService = seasonService;
            _matchService = matchService;
        }

        public Player? PlayerMe { get; set; }
        public List<MatchDTO> MatchesDTO { get; set; } = new();
        public SeasonDTO? SeasonDTO { get; set; }
        public bool ShowSeasonModal { get; set; } = false;
        public Season NewSeason { get; set; } = new();
        public PlayerSeason PlayerSeason { get; set; } = new();
        public decimal CounterOfVictory { get; set; } = 0;
        public decimal CounterOfVictoryWithSet { get; set; } = 0;
        public decimal CounterOfDefeat { get; set; } = 0;
        public decimal CounterOfDefeatWithSet { get; set; } = 0;
        public decimal CounterOfWithdraw { get; set; } = 0;
        public decimal? PointsBeginningSeason { get; set; }
        public decimal? PointsMiddleOfSeason { get; set; }
        public decimal? VirtualPoints { get; set; } = 0;
        public decimal? MonthlyPoints { get; set; }
        public decimal? VictoryPercentage { get; set; } = 0;
        public decimal LargestPointGapWithVictory { get; set; }
        public decimal LargestPointGap { get; set; }
        public string ResultLargestPointGap { get; set; }
        public decimal SmallestPointGap { get; set; }
        public decimal SmallestPointGapWithDefeat { get; set; }
        public string ResultSmallestPointGap { get; set; }
        public decimal MatchWithSets { get; set; } = 0;
        public List<SelectListItem> SeasonsSelectList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SelectedSeasonId { get; set; }

        public Dictionary<int?, int> VictoriesNbOfSets = new() { { 3, 0 }, { 4, 0 }, { 5, 0 } };
        public Dictionary<int?, int> DefeatsNbOfSets = new() { { 3, 0 }, { 4, 0 }, { 5, 0 } };
        public Dictionary<string, int> DetailedResults { get; set; } = new();
        public string[] MonthLabels { get; set; }
        public decimal?[] MonthValues { get; set; }
        public int RankingMaxValue { get; set; }
        public Dictionary<int, List<MatchDTO>> SortedMatchesDTO = new()
        {
            { 9, new() }, { 10, new() }, { 11, new() }, { 12, new() },
            { 1, new() }, { 2, new() }, { 3, new() }, { 4, new() },
            { 5, new() }, { 6, new() }, { 7, new() }, { 8, new() }
        };
        public Dictionary<DateOnly, decimal?> MonthliesPointsOrdered { get; set; } = new();
        public Dictionary<DateOnly, decimal?> MonthliesPointsOrderedFiltered { get; set; } = new();
        public Dictionary<int, int> VictoryDistributionOrdered = new();
        public Dictionary<int, int> DefeatDistributionOrdered = new();
        public int VictoryDefeatMaxGauge { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var seasons = await _seasonService.GetAllSeasonsAsync();

            SeasonsSelectList = seasons
                .OrderByDescending(s => s.Start_date)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();

            if (SelectedSeasonId == 0)
            {
                var sessionValue = HttpContext.Session.GetString("SelectedSeasonId");

                if (sessionValue != null && int.TryParse(sessionValue, out int sessionSeasonId)
                    && seasons.Any(s => s.Id == sessionSeasonId))
                {
                    SelectedSeasonId = sessionSeasonId;
                    SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(seasons.First(s => s.Id == sessionSeasonId));
                }
                else
                {
                    var currentSeason = await _seasonService.GetCurrentSeasonAsync();
                    if (currentSeason == null)
                    {
                        ShowSeasonModal = true;
                        return;
                    }
                    SelectedSeasonId = currentSeason.Id;
                    SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(currentSeason);
                }
            }
            else
            {
                var selectedSeason = seasons.First(s => s.Id == SelectedSeasonId);
                SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(selectedSeason);
                HttpContext.Session.SetString("SelectedSeasonId", SelectedSeasonId.ToString());
            }

            var data = await _indexService.GetIndexDataAsync(SelectedSeasonId);

            // Mapping des données
            PlayerMe = data.PlayerMe;
            PlayerSeason = data.PlayerSeason;
            MatchesDTO = data.MatchesDTO;
            PointsBeginningSeason = data.PointsBeginningSeason;
            PointsMiddleOfSeason = data.PointsMiddleOfSeason;
            MonthlyPoints = data.MonthlyPoints;
            VirtualPoints = data.VirtualPoints;
            VictoryPercentage = data.VictoryPercentage;
            CounterOfVictory = data.CounterOfVictory;
            CounterOfVictoryWithSet = data.CounterOfVictoryWithSet;
            CounterOfDefeat = data.CounterOfDefeat;
            CounterOfDefeatWithSet = data.CounterOfDefeatWithSet;
            CounterOfWithdraw = data.CounterOfWithdraw;
            LargestPointGap = data.LargestPointGap;
            ResultLargestPointGap = data.ResultLargestPointGap;
            SmallestPointGap = data.SmallestPointGap;
            ResultSmallestPointGap = data.ResultSmallestPointGap;
            LargestPointGapWithVictory = data.LargestPointGapWithVictory;
            SmallestPointGapWithDefeat = data.SmallestPointGapWithDefeat;
            MatchWithSets = data.MatchWithSets;
            DetailedResults = data.DetailedResults;
            VictoriesNbOfSets = data.VictoriesNbOfSets;
            DefeatsNbOfSets = data.DefeatsNbOfSets;
            SortedMatchesDTO = data.SortedMatchesDTO;
            MonthliesPointsOrdered = data.MonthliesPointsOrdered;
            MonthliesPointsOrderedFiltered = data.MonthliesPointsOrderedFiltered;
            VictoryDistributionOrdered = data.VictoryDistributionOrdered;
            DefeatDistributionOrdered = data.DefeatDistributionOrdered;
            VictoryDefeatMaxGauge = data.VictoryDefeatMaxGauge;
            MonthLabels = data.MonthLabels;
            MonthValues = data.MonthValues;
            RankingMaxValue = data.RankingMaxValue;
        }

        public async Task<IActionResult> OnPostCreateSeasonAutomaticallyAsync()
        {
            await _indexService.CreateSeasonAutomaticallyAsync();
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int matchId)
        {
            await _matchService.DeleteMatchAsync(matchId);
            return RedirectToPage();
        }
    }
}