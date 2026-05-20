using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class SeasonService
    {
        private readonly TableTennisHistoricDbContext _context;

        public SeasonService(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public async Task<Season> GetSeasonByIdAsync(int id)
        {
            return await _context.Season.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Season>?> GetAllSeasonsAsync()
        {
            return await _context.Season.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Season?> GetSeasonByDateAsync(DateOnly date)
        {
            int year = (date.Month >= 9) ? date.Year : date.Year - 1;
            DateOnly start_date = new DateOnly(year, 9, 1);
            Season? season = await _context.Season.FirstOrDefaultAsync(s => s.Start_date == start_date);
            return season;
        }

        public async Task<Season?> GetCurrentSeasonAsync()
        {
            DateOnly date = DateOnly.FromDateTime(DateTime.Now);
            int year = (date.Month >= 9) ? date.Year : date.Year - 1;
            DateOnly start_date = new DateOnly(year, 9, 1);
            Season? season = await _context.Season.FirstOrDefaultAsync(s => s.Start_date == start_date);
            return season;
        }

        public SeasonDTO ConvertSeasonToSeasonDTO(Season season)
        {
            SeasonDTO seasonDTO = new SeasonDTO
            {
                Name = season.Name,
                Start_date = season.Start_date,
                End_date = season.End_date,
            };

            return seasonDTO;
        }
    }
}
