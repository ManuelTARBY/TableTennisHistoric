using System.ComponentModel.DataAnnotations;

namespace TableTennisHistoric.Models
{
    public class Club
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le numéro de licence du club est obligatoire.")]
        [StringLength(10, ErrorMessage = "Le numéro de licence ne peut pas dépasser 10 caractères.")]
        public string License_number { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le nom du club est obligatoire.")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom abrégé du club est obligatoire.")]
        [StringLength(30, ErrorMessage = "Le nom ne peut pas dépasser 30 caractères.")]
        public string Name_abrev { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville du club est obligatoire.")]
        [StringLength(100, ErrorMessage = "La ville ne peut pas dépasser 100 caractères.")]
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "La ville du club est obligatoire.")]
        public int Department { get; set; } = 0;

        public List<PlayerSeason> PlayerSeasons { get; set; } = new();
        //public List<PlayerClub> PlayerClubs { get; set; } = new();
    }
}
