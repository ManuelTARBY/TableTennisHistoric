namespace TableTennisHistoric.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string? License_number { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public Boolean Is_me { get; set; }
        public List<PlayerSeason> PlayerSeasons { get; set; } = new();
        public List<TableTennisMatch> TableTennisMatchs { get; set; } = new();
    }
}
