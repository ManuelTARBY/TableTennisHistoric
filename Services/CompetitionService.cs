using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class CompetitionService
    {

        private readonly TableTennisHistoricDbContext _context;

        public CompetitionService(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompetitionDTO>>? GetAllCompetitionDTOAsync()
        {
            return await _context.Competition
                .Select(c => new CompetitionDTO
                {
                    Name = c.Name
                })
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Competition>>? GetAllCompetitionAsync()
        {
            return await _context.Competition
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Competition?> GetCompetitionByIdAsync(int id)
        {
            Competition? competition = await _context.Competition.FirstOrDefaultAsync(c => c.Id == id);
            return competition;
        }

        public async Task<CompetitionCoefficient?> GetCompetitionCoefficientByIdAsync(int id)
        {
            return await _context.CompetitionCoefficient.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Competition> GetCompetitionByCompetitionCoefficientIdAsync(int id)
        {
            CompetitionCoefficient? competitionCoefficient = await _context.CompetitionCoefficient.FirstOrDefaultAsync(c => c.Id == id);
            
            if (competitionCoefficient == null) { return null; }

            Competition? competition = await _context.Competition.FirstOrDefaultAsync(c => c.Id == competitionCoefficient.CompetitionId);

            return competition;
        }

        public async Task<List<SeasonCompetitionDTO>> GetCompetitionDTOBySeasonAsync(int seasonId)
        {
            return await _context.CompetitionCoefficient
                .Where(cc => cc.SeasonId == seasonId)
                .Select(cc => new SeasonCompetitionDTO
                {
                    CompetitionName = cc.Competition.Name,
                    CompetitionCoefficient = cc.Coefficient
                })
                .OrderBy(cc => cc.CompetitionName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CompetitionDTO?> GetCompetitionDTOById(int id)
        {
            Competition? competition = await GetCompetitionByIdAsync(id);

            if (competition == null) { return null; }

            return ConvertCompetitionToCompetitionDTO(competition);
        }

        public CompetitionDTO? ConvertCompetitionToCompetitionDTO(Competition competition)
        {
            
            if (competition == null) { return null; }
            
            CompetitionDTO competitionDTO = new CompetitionDTO
            {
                Name = competition.Name,
            };

            return competitionDTO;
        }
    }
}
