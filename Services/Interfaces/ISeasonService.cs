using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface ISeasonService
    {
        Task<Season> GetSeasonByIdAsync(int id);
        Task<List<Season>?> GetAllSeasonsAsync();
        Task<Season?> GetSeasonByDateAsync(DateOnly date);
        Task<Season?> GetCurrentSeasonAsync();
        SeasonDTO ConvertSeasonToSeasonDTO(Season season);
        Task<List<SeasonEditDTO>> GetAllSeasonEditDTOAsync();
        Task<(bool Success, string? Error)> UpdateSeasonAsync(SeasonEditDTO dto);
        Task<(bool Success, string? Error)> CreateSeasonAsync(Season season);
    }
}
