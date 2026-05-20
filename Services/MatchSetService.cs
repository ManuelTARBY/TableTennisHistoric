using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class MatchSetService
    {
        private readonly TableTennisHistoricDbContext _context;

        public MatchSetService(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetNbOfSetAsync(TableTennisMatch match)
        {
            if (match == null) return null;

            // On filtre les sets pour ce match et on compte
            return await _context.MatchSet
                                 .Where(ms => ms.MatchId == match.Id)
                                 .CountAsync();
        }

        public async Task<List<MatchSet>?> GetMatchSetByMatchId(int id)
        {
            return await _context.MatchSet.Where(ms => ms.MatchId == id).ToListAsync();
        }
    }
}
