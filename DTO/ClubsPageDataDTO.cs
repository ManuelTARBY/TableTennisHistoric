using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class ClubsPageDataDTO
    {
        public List<Club> Clubs { get; set; } = new();
        public SelectList ClubSelectList { get; set; } = null!;
        public List<Team> SelectedClubTeams { get; set; } = new();
    }
}
