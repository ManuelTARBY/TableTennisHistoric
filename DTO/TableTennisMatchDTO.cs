using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class TableTennisMatchDTO
    {
        public DateTime Date_match { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public Decimal Competition_coefficient { get; set; }
        public required string Player_first_name { get; set; }
        public required string Player_last_name { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public Decimal My_points_at_match { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public Decimal Opponent_points_at_match { get; set; }
        public string? Comment { get; set; } = null;
        public string? Stage_name { get; set; }
        public enum MatchResult { V, D, F }
        public MatchResult Result { get; set; }
        public string? Sets_detail { get; set; }
    }
}
