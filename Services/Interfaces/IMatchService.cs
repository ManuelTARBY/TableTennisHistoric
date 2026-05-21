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
        Task<TableTennisMatch?> GetMatchWithSetsAsync(int id);
        Task<(List<SelectListItem> CompetitionCoefficients, List<SelectListItem> Opponents, List<SelectListItem> Stages)> GetUpdateMatchSelectListsAsync(TableTennisMatch match);
        List<string> ValidateSets(List<MatchSet> sets, TableTennisMatch.MatchResult result);
        Task UpdateMatchAsync(TableTennisMatch match, int competitionCoefficientId, int? stageId,
            int opponentId, DateTime dateMatch, decimal myPoints, decimal opponentPoints,
            TableTennisMatch.MatchResult result, string? comment, List<MatchSet> sets);
        decimal ComputeFromCalculator(decimal myPoints, decimal opponentPoints, decimal coefficient, bool isVictory);
    }
}
