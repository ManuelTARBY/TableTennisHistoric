using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class SeasonService: ISeasonService
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

        public async Task<List<SeasonEditDTO>> GetAllSeasonEditDTOAsync()
        {
            return await _context.Season
                .OrderByDescending(s => s.Start_date)
                .Select(s => new SeasonEditDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Start_date = s.Start_date,
                    End_date = s.End_date,
                    Phase1_End_date = s.Phase1_End_date,
                    p1_drift = s.p1_drift,
                    p2_drift = s.p2_drift
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error)> UpdateSeasonAsync(SeasonEditDTO dto)
        {
            var season = await _context.Season.FindAsync(dto.Id);
            if (season == null)
                return (false, "Saison introuvable.");

            season.Start_date = dto.Start_date;
            season.End_date = dto.End_date;
            season.Phase1_End_date = dto.Phase1_End_date;
            season.p1_drift = dto.p1_drift ?? 0;
            season.p2_drift = dto.p2_drift ?? 0;

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
