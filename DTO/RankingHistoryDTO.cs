namespace TableTennisHistoric.DTO
{
    public class RankingHistoryDTO
    {
        public Dictionary<DateOnly, decimal?> RankingHistory { get; set; } = new();
        public int RankingMaxValue { get; set; }
    }
}
