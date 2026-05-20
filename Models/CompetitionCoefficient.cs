using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class CompetitionCoefficient
    {
        [Column("id")]
        public int Id { get; set; }
        [Required(ErrorMessage = "La compétition est obligatoire")]
        public int CompetitionId { get; set; }
        [JsonIgnore]
        public Competition Competition { get; set; }
        [Required(ErrorMessage = "La saison est obligatoire")]
        public int SeasonId { get; set; }
        [JsonIgnore]
        public Season Season { get; set; }
        [Required(ErrorMessage = "Le coefficient est obligatoire")]
        [Column(TypeName = "decimal(4,2)")]
        public decimal Coefficient { get; set; }
        public List<TableTennisMatch> TableTennisMatchs { get; set; } = new();
    }
}
