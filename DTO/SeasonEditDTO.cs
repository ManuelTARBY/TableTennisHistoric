using System.ComponentModel.DataAnnotations;

namespace TableTennisHistoric.DTO
{
    public class SeasonEditDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        [Required]
        public DateOnly Start_date { get; set; }

        [Required]
        public DateOnly End_date { get; set; }

        [Required]
        public DateOnly Phase1_End_date { get; set; }

        public decimal? p1_drift { get; set; }
        public decimal? p2_drift { get; set; }
    }
}