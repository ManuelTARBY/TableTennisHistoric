using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class PlayerSeason
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        [JsonIgnore]
        public Player Player { get; set; }
        public int SeasonId { get; set; }
        [JsonIgnore]
        public Season Season { get; set; }
        public int ClubId { get; set; }
        [JsonIgnore]
        public Club Club { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal? Points_start { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal? Points_middle { get; set; }
        public PlayerCategory? Category { get; set; }
        public enum PlayerCategory {
            Poussin,
            [Display(Name = "Benjamin 1")]
            Benjamin1,
            [Display(Name = "Benjamin 2")]
            Benjamin2,
            [Display(Name = "Minime 1")]
            Minime1,
            [Display(Name = "Minime 2")]
            Minime2,
            [Display(Name = "Cadet 1")]
            Cadet1,
            [Display(Name = "Cadet 2")]
            Cadet2,
            [Display(Name = "Junior 1")]
            Junior1,
            [Display(Name = "Junior 2")]
            Junior2, 
            [Display(Name = "Junior 3")]
            Junior3,
            [Display(Name = "Junior 4")]
            Junior4,
            Senior,
            [Display(Name = "Vétéran 40")]
            Vétéran40,
            [Display(Name = "Vétéran 45")]
            Vétéran45,
            [Display(Name = "Vétéran 50")]
            Vétéran50,
            [Display(Name = "Vétéran 55")]
            Vétéran55, 
            [Display(Name = "Vétéran 60")]
            Vétéran60,
            [Display(Name = "Vétéran 65")]
            Vétéran65,
            [Display(Name = "Vétéran 70")]
            Vétéran70,
            [Display(Name = "Vétéran 75")]
            Vétéran75,
            [Display(Name = "Vétéran 80")]
            Vétéran80,
            [Display(Name = "Vétéran 85")]
            Vétéran85,
            [Display(Name = "Vétéran 90")]
            Vétéran90
        }
    }
}
