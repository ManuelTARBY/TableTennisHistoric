using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IPlayerService
    {
        PlayerDTO? ConvertPlayerToPlayerDTO(Player player);
        Task<(bool Success, string? Error, int? PlayerId)> CreatePlayerWithAffiliationAsync(Player player, PlayerSeason playerSeason);
        Task<decimal?> GetPointsBeginningOfCurrentSeasonAsync(Player player);
        Task<decimal?> GetPointsMiddleOfCurrentSeasonAsync(Player player);
        Task<decimal?> GetPointsBeginningOfSeasonAsync(Player player, Season season);
        Task<decimal?> GetPointsMiddleOfSeasonAsync(Player player, Season season);
        Task<Player?> GetMeAsync();
        Task<List<Player>?> GetAllPlayersAsync();
        Task<Club?> GetClubByPlayerIdAndSeasonIdAsync(int playerId, int seasonId);
        Task<Club?> GetClubByPlayerAndSeasonAsync(Player player, Season season);
        Task<Club?> GetCurrentClubAsync(Player player);
        Task<PlayerSeason?> GetPlayerSeasonByPlayerIdAndSeasonIdAsync(int playerId, int seasonId);
        Task<PlayerSeason> GetPlayerSeasonByPlayerAndSeasonAsync(Player player, Season season);
        Task<PlayerSeason?> GetPlayerSeasonAsync(Player player, Season season);
        Task<List<PlayerWithClubDTO>> GetPlayersWithClubBySeasonAsync(Season season);
        Task<RankingHistoryDTO> GetRankingHistoryByPlayerIdAsync(int? playerId);
        Task<SelectList> GetOpponentsSelectListAsync();
        Task<(bool Success, string? Error)> CreatePlayerSeasonAsync(PlayerSeason playerSeason);
        Task CreatePlayerAsync(Player player);
        Task<(SelectList Players, SelectList Clubs, SelectList Seasons)> GetCreatePlayerSelectListsAsync();
        Task<List<PlayerSeasonDTO>> GetPlayerSeasonsByPlayerIdAsync(int playerId);
        Task UpdatePlayerSeasonAsync(PlayerSeasonDTO dto);
        Task DeletePlayerSeasonAsync(int id);
        Task<List<PlayerEditDTO>> GetAllPlayerEditDTOAsync();
        Task<(bool Success, string? Error)> UpdatePlayerAsync(PlayerEditDTO dto);
        Task<PlayerDTO?> GetPlayerByLicenseNumberAsync(string licensenumber);
    }
}
