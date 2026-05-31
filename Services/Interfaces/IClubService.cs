using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IClubService
    {
        Task<Club?> GetClubByIdAsync(int id);
        ClubDTO ConvertClubToClubDTO(Club club);
        Task<ClubsPageDataDTO> GetClubsPageDataAsync(int? selectedClubId);
        Task CreateClubAsync(Club club);
        Task CreateTeamsAsync(int clubId, string baseName, int count);
        Task<(bool Success, string? Error)> UpdateClubAsync(int id, Club updated);
        Task<(bool Success, string? Error)> DeleteClubAsync(int id);
        Task<SelectList> GetClubsSelectListAsync();
    }
}
