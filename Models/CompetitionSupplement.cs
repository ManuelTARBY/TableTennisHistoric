using System.ComponentModel.DataAnnotations;

namespace TableTennisHistoric.Models
{
    public class CompetitionSupplement
    {
        public int Id { get; set; }
        [StringLength(20, ErrorMessage = "Le numéro de licence ne peut pas dépasser 20 caractères.")]
        public string Name { get; set; }
    }
}
