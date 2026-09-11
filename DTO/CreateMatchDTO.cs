using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class CreateMatchDTO
    {
        public DateTime Date_match { get; set; }
        public int CompetitionCoefficientId { get; set; }
        public int? CompetitionSupplementId { get; set; }
        public int? StageId { get; set; }
        public int OpponentId { get; set; }
        public decimal My_points_at_match { get; set; }
        public decimal Opponent_points_at_match { get; set; }
        public TableTennisMatch.MatchResult Result { get; set; }
        public decimal Points_won { get; set; }
        public string? Comment { get; set; }
        public List<int?> SetMy { get; set; } = new();
        public List<int?> SetOpp { get; set; } = new();
    }
}
