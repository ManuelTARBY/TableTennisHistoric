using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class MatchSet
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        [JsonIgnore]
        public TableTennisMatch Match { get; set; } = null!;
        public int SetNumber { get; set; }
        public int Player1Score { get; set; }
        public int Player2Score { get; set; }
    }
}
