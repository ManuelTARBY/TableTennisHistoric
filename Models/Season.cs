using System.ComponentModel.DataAnnotations.Schema;

namespace TableTennisHistoric.Models
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly Start_date { get; set; }
        public DateOnly End_date { get; set; }
        public DateOnly Phase1_End_date { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal? p1_drift { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal? p2_drift { get; set; }
        public List<CompetitionCoefficient> CompetitionCoefficients { get; set; } = new();
        public List<PlayerSeason> SeasonPoints { get; set; } = new();
    }
}
