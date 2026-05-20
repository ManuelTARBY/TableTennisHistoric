using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class ChampionshipService
    {
        private readonly TableTennisHistoricDbContext _context;
        public SeasonService _seasonService { get; set; }

        public ChampionshipService(TableTennisHistoricDbContext context, SeasonService seasonService)
        {
            _context = context;
            _seasonService = seasonService;
        }
        public async Task<List<Championship>?> GetAllChampionShips()
        {
            return await _context.Championship.ToListAsync();
        }

        public async Task<Championship?> GetChampionShipByIdAsync(int id)
        {
            return await _context.Championship.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Championship>?> GetChampionshipsBySeason(Season season)
        {
            return await _context.Championship.Where(c => c.Season == season).ToListAsync();
        }

        public ChampionshipDTO ConvertChampionShipToChampionshipDTO(Championship championship)
        {
            Season season = _context.Season.FirstOrDefault(s => s.Id == championship.SeasonId);

            return new ChampionshipDTO
            {
                   Season = season.Name,
                   Rank = championship.Rank,
                   Level = championship.Level,
                   ChampionshipGroup = championship.ChampionshipGroup,
                   Phase = championship.Phase,
            };

        }
    }
}
