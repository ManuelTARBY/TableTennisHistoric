using System.ComponentModel.DataAnnotations;

namespace TableTennisHistoric.Models
{
    public class Competition
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom de la compétition est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom de la compétition ne peut pas dépasser 100 caractères.")]
        public string Name { get; set; } = string.Empty;
        public List<CompetitionCoefficient> CompetitionCoefficients { get; set; } = new();
    }
}
