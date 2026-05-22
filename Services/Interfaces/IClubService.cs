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
    }
}
