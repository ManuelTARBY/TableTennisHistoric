namespace TableTennisHistoric.DTO
{
    public class RankingHistoryDTO
    {
        public Dictionary<string, decimal?> RankingHistory { get; set; } = new();
        public int RankingMaxValue { get; set; }
    }
}
