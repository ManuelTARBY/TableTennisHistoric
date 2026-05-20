using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required]
        public int ClubId { get; set; }
        [JsonIgnore]
        public Club Club { get; set; }
        [Required(ErrorMessage = "Le nom de l'équipe est obligatoire.")]
        public string Name { get; set; }
        public List<ChampionshipTeam> ChampionshipTeams { get; set; } = new();
    }
}
