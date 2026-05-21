using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IMatchService
    {
        Task<List<TableTennisMatch>?> GetAllMatchesAsync();
        Task<List<TableTennisMatch>?> GetAllMatchesByDateDescendingAsync();
        Task<List<MatchDTO>> GetAllMatchesDTOBySeasonAsync(Season season);
        Task<List<MatchDTO>> GetAllMatchesDTOAsync();
        Task<MatchDTO> ConvertMatchToMatchDTOAsync(TableTennisMatch match);
        Task<List<MatchSet>?> GetMatchSetsAsync(TableTennisMatch match);
        void DetermineTypeOfResult(MatchDTO match, ref Dictionary<string, int> DetailedResults);
        List<SetDTO> ConvertMatchSetsToMatchSetDTOs(List<MatchSet> matchSets);
        decimal ComputeDTO(MatchDTO match, decimal coefficient);
        decimal Compute(TableTennisMatch match, CompetitionCoefficient? competitionCoefficient);
        Task<bool> CreateMatchAsync(int competitionCoefficientId, int opponentId, TableTennisMatch match);
        Task<(List<SelectListItem> CompetitionCoefficients, List<SelectListItem> Opponents)> GetCreateMatchSelectListsAsync();
    }
}
