using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using static TableTennisHistoric.DTO.ChampionshipDTO;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IChampionshipService
    {
        Task<List<Championship>?> GetAllChampionShips();
        Task<Championship?> GetChampionShipByIdAsync(int id);
        Task<List<Championship>?> GetChampionshipsBySeason(Season season);
        ChampionshipDTO ConvertChampionShipToChampionshipDTO(Championship championship);
        Task<(SelectList SeasonSelectList, List<Championship> Championships, List<Club> Clubs)> GetCreatePageDataAsync();
        Task CreateChampionshipAsync(Championship championship);
        Task SaveTeamsAndGenerateMatchesAsync(int championshipId, List<ChampionshipTeam> teamLines);
        Task<List<object>> GetTeamsByClubAsync(int clubId);
        Task<List<SelectListItem>> GetChampionshipSelectListAsync();
        Task SaveScoresAsync(List<(int Id, int? HomeScore, int? AwayScore)> scores);
        Task<ChampionshipPageData> GetChampionshipDataAsync(int id);
    }
}
