using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services.Interfaces
{
    public interface IMatchSetService
    {
        Task<int?> GetNbOfSetAsync(TableTennisMatch match);
        Task<List<MatchSet>?> GetMatchSetByMatchId(int id);
    }
}
