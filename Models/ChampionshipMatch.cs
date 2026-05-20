using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class ChampionshipMatch
    {
        public int Id { get; set; }
        public int ChampionshipId { get; set; }
        [JsonIgnore]
        public Championship Championship { get; set; }
        public int Matchday { get; set; }
        public int? Home_score { get; set; }
        public int? Away_score { get; set; }
        public int Home_teamId { get; set; }
        [JsonIgnore]
        public Team Home_team { get; set; }
        public int Away_teamId { get; set; }
        [JsonIgnore]
        public Team Away_team { get; set; }

    }
}
