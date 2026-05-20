using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TableTennisHistoric.Models
{
    public class PlayerClub
    {
        public int Player_id { get; set; }
        [JsonIgnore]
        public Player Player { get; set; }
        public int Club_id { get; set; }
        [JsonIgnore]
        public Club Club { get; set; }
        public int Season_id { get; set; }
        [JsonIgnore]
        public Season Season { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal? Point_beginning_of_season { get; set; }
    }
}
