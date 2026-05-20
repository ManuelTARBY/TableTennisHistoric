using System.ComponentModel.DataAnnotations.Schema;

namespace TableTennisHistoric.DTO
{
    public class SeasonCompetitionDTO
    {
        public string CompetitionName { get; set; }
        [Column(TypeName = "decimal(4,2)")]
        public decimal CompetitionCoefficient { get; set; }
    }
}
