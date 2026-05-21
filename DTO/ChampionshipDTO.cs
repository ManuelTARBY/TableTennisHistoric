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
        public class ChampionshipMatchData
        {
            public int Id { get; set; }
            public int Matchday { get; set; }
            public int? Home_score { get; set; }
            public int? Away_score { get; set; }
            public string? HomeTeamName { get; set; }
            public string? AwayTeamName { get; set; }
        }

        public class ChampionshipTeamResult
        {
            public string Name { get; set; } = "";
            public int NbOfGamePlayed { get; set; } = 0;
            public int Points { get; set; } = 0;
            public int Victory { get; set; } = 0;
            public int Draw { get; set; } = 0;
            public int Defeat { get; set; } = 0;
            public int? GameWon { get; set; } = 0;
            public int? GameLost { get; set; } = 0;
            public int? GameAverage { get; set; } = 0;
        }

        public class ChampionshipPageData
        {
            public List<ChampionshipMatchData> Matches { get; set; } = new();
            public Championship? Championship { get; set; }
            public List<ChampionshipTeamResult> TeamResults { get; set; } = new();
        }
    }
}
