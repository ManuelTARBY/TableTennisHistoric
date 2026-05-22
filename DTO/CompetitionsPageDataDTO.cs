using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class CompetitionsPageDataDTO
    {
        public Season? CurrentSeason { get; set; }
        public List<Season>? AllSeasons { get; set; } = new();
        public List<Competition>? Competitions { get; set; } = new();
        public List<SeasonCompetitionDTO>? CompetitionWithCoefficient { get; set; } = new();
        public SelectList SeasonList { get; set; } = null!;
        public SelectList CompetitionList { get; set; } = null!;
    }
}
