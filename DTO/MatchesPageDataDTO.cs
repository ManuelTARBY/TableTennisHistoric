using Microsoft.AspNetCore.Mvc.Rendering;

namespace TableTennisHistoric.DTO
{
    public class MatchesPageDataDTO
    {
        public List<MatchDTO> MatchesDTO { get; set; } = new();
        public IEnumerable<SelectListItem> CompetitionCoefficients { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Competitions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Opponents { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Stages { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> CompetitionSupplements { get; set; } = new List<SelectListItem>();
    }
}
