using System.ComponentModel.DataAnnotations;

namespace TableTennisHistoric.Models
{
    public class Stage
    {
        public int Id { get; set; }
        [StringLength(80, ErrorMessage = "Le nom du stade ne peut pas dépasser 80 caractères.")]
        public string Name { get; set; } = string.Empty;
        public List<TableTennisMatch> TableTennisMatchs { get; set; } = new();
    }
}
