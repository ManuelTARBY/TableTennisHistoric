using System.ComponentModel.DataAnnotations.Schema;

namespace TableTennisHistoric.DTO
{
    public class SeasonCompetitionDTO
    {
        public int Id { get; set; }
        public string CompetitionName { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal CompetitionCoefficient { get; set; }
    }
}
