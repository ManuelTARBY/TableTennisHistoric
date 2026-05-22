using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class IndexDataDTO
    {
        public Player? PlayerMe { get; set; }
        public PlayerSeason PlayerSeason { get; set; } = new();
        public List<MatchDTO> MatchesDTO { get; set; } = new();
        public decimal? PointsBeginningSeason { get; set; }
        public decimal? PointsMiddleOfSeason { get; set; }
        public decimal? MonthlyPoints { get; set; }
        public decimal? VirtualPoints { get; set; }
        public decimal? VictoryPercentage { get; set; }
        public decimal CounterOfVictory { get; set; }
        public decimal CounterOfVictoryWithSet { get; set; }
        public decimal CounterOfDefeat { get; set; }
        public decimal CounterOfDefeatWithSet { get; set; }
        public decimal CounterOfWithdraw { get; set; }
        public decimal LargestPointGap { get; set; }
        public string ResultLargestPointGap { get; set; }
        public decimal SmallestPointGap { get; set; }
        public string ResultSmallestPointGap { get; set; }
        public decimal LargestPointGapWithVictory { get; set; }
        public decimal SmallestPointGapWithDefeat { get; set; }
        public decimal MatchWithSets { get; set; }
        public Dictionary<string, int> DetailedResults { get; set; } = new();
        public Dictionary<int?, int> VictoriesNbOfSets { get; set; } = new() { { 3, 0 }, { 4, 0 }, { 5, 0 } };
        public Dictionary<int?, int> DefeatsNbOfSets { get; set; } = new() { { 3, 0 }, { 4, 0 }, { 5, 0 } };
        public Dictionary<int, List<MatchDTO>> SortedMatchesDTO { get; set; } = new()
        {
            { 9, new() }, { 10, new() }, { 11, new() }, { 12, new() },
            { 1, new() }, { 2, new() }, { 3, new() }, { 4, new() },
            { 5, new() }, { 6, new() }, { 7, new() }, { 8, new() }
        };
        public Dictionary<DateOnly, decimal?> MonthliesPointsOrdered { get; set; } = new();
        public Dictionary<DateOnly, decimal?> MonthliesPointsOrderedFiltered { get; set; } = new();
        public Dictionary<int, int> VictoryDistributionOrdered { get; set; } = new();
        public Dictionary<int, int> DefeatDistributionOrdered { get; set; } = new();
        public int VictoryDefeatMaxGauge { get; set; }
        public string[] MonthLabels { get; set; } = Array.Empty<string>();
        public decimal?[] MonthValues { get; set; } = Array.Empty<decimal?>();
        public int RankingMaxValue { get; set; }
    }
}
