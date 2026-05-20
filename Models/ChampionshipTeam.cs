using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class ChampionshipTeam
    {
        public int Id { get; set; }

        [Required]
        public int ChampionshipId { get; set; }
        [JsonIgnore]
        public Championship Championship { get; set; }
        [Required]
        public int TeamId { get; set; }
        [JsonIgnore]
        public Team Team { get; set; }
        public int Number { get; set; }
    }
}
