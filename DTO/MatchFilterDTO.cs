using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class MatchFilterDTO
    {
        public int? SeasonId { get; set; }
        public int? CompetitionId { get; set; }
        public int? ClubId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public TableTennisMatch.MatchResult? Result { get; set; }
        public int? NbOfSets { get; set; }
        public decimal? OpponentPointsMin { get; set; }
        public decimal? OpponentPointsMax { get; set; }
    }
}
