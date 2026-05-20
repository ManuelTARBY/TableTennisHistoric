using System.ComponentModel.DataAnnotations.Schema;

namespace TableTennisHistoric.DTO
{
    public class SeasonDTO
    {
        public string Name { get; set; }
        public DateOnly Start_date { get; set; }
        public DateOnly End_date { get; set; }
        public DateOnly Phase1_End_date { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal? p1_drift { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal? p2_drift { get; set; }
    }
}
