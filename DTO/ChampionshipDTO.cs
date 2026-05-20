using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class ChampionshipDTO
    {
        public string Season { get; set; }
        public int Rank { get; set; }
        public static readonly Dictionary<int, string> RankLabels = new()
        {
            { 1, "National" },
            { 2, "Régional" },
            { 3, "Départemental" }
        };

        public int Level { get; set; }
        public int ChampionshipGroup { get; set; }
        public int Phase { get; set; }
        public DateOnly? Beginning { get; set; }
        public DateOnly? Ending { get; set; }
        public List<TeamDTO> ChampionShipTeams { get; set; } = new();
    }
}
