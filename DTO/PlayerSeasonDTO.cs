using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class PlayerSeasonDTO
    {
        public int Id { get; set; }
        public string PlayerFullName { get; set; } = "";
        public string SeasonName { get; set; } = "";
        public string ClubName { get; set; } = "";
        public PlayerSeason.PlayerCategory? Category { get; set; }
        public decimal? Points_start { get; set; }
        public decimal? Points_middle { get; set; }
        public int PlayerId { get; set; }
        public int SeasonId { get; set; }
        public int ClubId { get; set; }
    }
}
