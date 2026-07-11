using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class TableTennisMatch
    {
        [Key]
        public int Id { get; set; }

        public DateOnly Date_match { get; set; }

        public int CompetitionCoefficientId { get; set; }
        [JsonIgnore]
        public CompetitionCoefficient CompetitionCoefficient { get; set; } = null!;

        public int OpponentId { get; set; }
        [JsonIgnore]
        public Player Opponent { get; set; } = null!;

        public int? CompetitionSupplementId { get; set; } = null;
        [JsonIgnore]
        public CompetitionSupplement? CompetitionSupplement { get; set; } = null;
        public int? StageId { get; set; } = null;
        [JsonIgnore]
        public Stage? Stage { get; set; } = null;
        [Column(TypeName = "decimal(6,2)")]
        public decimal My_points_at_match { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal Opponent_points_at_match { get; set; }

        public MatchResult Result { get; set; }

        public enum MatchResult
        {
            [Display(Name = "Victoire")] V,
            [Display(Name = "Défaite")] D,
            [Display(Name = "Forfait")] F
        }

        [Column(TypeName = "longtext")]
        public string? Comment { get; set; } = null;

        public List<MatchSet>? Sets { get; set; }
    }
}
