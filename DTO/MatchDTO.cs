using System.ComponentModel.DataAnnotations.Schema;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class MatchDTO
    {
        public int Id { get; set; }
        public DateOnly Date_of_match { get; set; }
        public string Competition { get; set; } = string.Empty;
        public CompetitionCoefficient CompetitionCoefficient { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal Coefficient { get; set; }
        public int SeasonId { get; set; }
        public string Season_name { get; set; } = string.Empty;
        public int OpponnentId { get; set; }
        public string Opponent_first_name { get; set; } = string.Empty;
        public string Opponent_last_name { get; set; } = string.Empty;
        public string Opponent_full_name { get; set; } = string.Empty;
        public string Opponent_club { get; set; } = string.Empty;
        [Column(TypeName = "decimal(6,2)")]
        public decimal My_points_at_match { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal Opponent_points_at_match { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal Point_difference { get; set; }
        public string? Comment { get; set; } = null;
        public string? Stage_name { get; set; } = null;
        public string? CompetitionSupplementName { get; set; } = null;
        public enum MatchResult { V, D, F }
        public MatchResult Result { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal Points_won { get; set; }
        public List<SetDTO>? MatchSets { get; set; }
    }
}
