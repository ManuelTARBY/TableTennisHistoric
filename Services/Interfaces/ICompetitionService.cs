using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface ICompetitionService
    {
        Task<List<CompetitionDTO>>? GetAllCompetitionDTOAsync();
        Task<List<Competition>>? GetAllCompetitionAsync();
        Task<List<CompetitionSupplement>>? GetAllCompetitionSupplementAsync();
        Task<Competition?> GetCompetitionByIdAsync(int id);
        Task<CompetitionCoefficient?> GetCompetitionCoefficientByIdAsync(int id);
        Task<CompetitionCoefficient?> GetCompetitionCoefficientByCompetitionAndSeasonAsync(int competitionId, int seasonId);
        Task<CompetitionCoefficient?> GetLastCompetitionCoefficientByCompetitionAsync(int competitionId);
        Task<Competition> GetCompetitionByCompetitionCoefficientIdAsync(int id);
        Task<List<SeasonCompetitionDTO>> GetCompetitionDTOBySeasonAsync(int seasonId);
        Task<CompetitionDTO?> GetCompetitionDTOById(int id);
        CompetitionDTO? ConvertCompetitionToCompetitionDTO(Competition competition);
        Task<CompetitionsPageDataDTO> GetCompetitionsPageDataAsync();
        Task<(bool Success, string? Error)> CreateCompetitionAsync(Competition competition);
        Task<(bool Success, string? Error)> CreateCompetitionCoefficientAsync(CompetitionCoefficient competitionCoefficient);
        Task<(bool Success, string? Error)> UpdateCoefficientCompetitionAsync(CoefficientCompetitionEditDTO dto);
    }
}
