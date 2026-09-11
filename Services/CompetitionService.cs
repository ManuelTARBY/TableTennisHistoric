using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly TableTennisHistoricDbContext _context;
        private readonly ISeasonService _seasonService;

        public CompetitionService(TableTennisHistoricDbContext context, ISeasonService seasonService)
        {
            _context = context;
            _seasonService = seasonService;
        }

        public async Task<List<CompetitionDTO>>? GetAllCompetitionDTOAsync()
        {
            return await _context.Competition
                .Select(c => new CompetitionDTO { Name = c.Name })
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

        public async Task<List<CompetitionSupplement>>? GetAllCompetitionSupplementAsync()
        {
            return await _context.CompetitionSupplement
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CompetitionCoefficient?>? GetCompetitionCoefficientByCompetitionAndSeasonAsync(int competitionId, int seasonId)
        {
            return await _context.CompetitionCoefficient
                .Where(cc => cc.CompetitionId == competitionId && cc.SeasonId == seasonId)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<CompetitionCoefficient?>? GetLastCompetitionCoefficientByCompetitionAsync(int competitionId)
        {
            return await _context.CompetitionCoefficient
                .Where(cc => cc.CompetitionId == competitionId)
                .OrderByDescending(cc => cc.Season.Start_date)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<Competition?> GetCompetitionByIdAsync(int id)
        {
            return await _context.Competition.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<CompetitionCoefficient?> GetCompetitionCoefficientByIdAsync(int id)
        {
            return await _context.CompetitionCoefficient.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Competition> GetCompetitionByCompetitionCoefficientIdAsync(int id)
        {
            CompetitionCoefficient? competitionCoefficient = await _context.CompetitionCoefficient.FirstOrDefaultAsync(c => c.Id == id);

            if (competitionCoefficient == null) return null;

            return await _context.Competition.FirstOrDefaultAsync(c => c.Id == competitionCoefficient.CompetitionId);
        }

        public async Task<List<SeasonCompetitionDTO>> GetCompetitionDTOBySeasonAsync(int seasonId)
        {
            return await _context.CompetitionCoefficient
                .Where(cc => cc.SeasonId == seasonId)
                .Select(cc => new SeasonCompetitionDTO
                {
                    Id = cc.Id,
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
            if (competition == null) return null;
            return ConvertCompetitionToCompetitionDTO(competition);
        }

        public CompetitionDTO? ConvertCompetitionToCompetitionDTO(Competition competition)
        {
            if (competition == null) return null;
            return new CompetitionDTO { Name = competition.Name };
        }

        public async Task<CompetitionsPageDataDTO> GetCompetitionsPageDataAsync()
        {
            var currentSeason = await _seasonService.GetCurrentSeasonAsync();
            var allSeasons = await _seasonService.GetAllSeasonsAsync();
            var competitions = await GetAllCompetitionAsync();

            var competitionWithCoefficient = currentSeason != null
                ? await GetCompetitionDTOBySeasonAsync(currentSeason.Id)
                : new List<SeasonCompetitionDTO>();

            var seasonList = new SelectList(
                await _context.Season
                    .OrderByDescending(s => s.Start_date)
                    .Select(s => new { s.Id, s.Name })
                    .ToListAsync(),
                "Id", "Name");

            var competitionList = new SelectList(
                await _context.Competition
                    .OrderBy(c => c.Name)
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync(),
                "Id", "Name");

            return new CompetitionsPageDataDTO
            {
                CurrentSeason = currentSeason,
                AllSeasons = allSeasons,
                Competitions = competitions,
                CompetitionWithCoefficient = competitionWithCoefficient,
                SeasonList = seasonList,
                CompetitionList = competitionList
            };
        }

        public async Task<(bool Success, string? Error)> CreateCompetitionAsync(Competition competition)
        {
            
            var exists = await _context.Competition.AnyAsync(c => c.Name == competition.Name);

            if (exists)
                return (false, "Cette compétition existe déjà.");

            try
            {
                _context.Competition.Add(competition);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Erreur lors de la sauvegarde : " + ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> CreateCompetitionCoefficientAsync(CompetitionCoefficient competitionCoefficient)
        {
            var exists = await _context.CompetitionCoefficient
                .AnyAsync(cc => cc.SeasonId == competitionCoefficient.SeasonId
                             && cc.CompetitionId == competitionCoefficient.CompetitionId);

            if (exists)
                return (false, "Cette compétition est déjà associée à cette saison.");

            try
            {
                _context.CompetitionCoefficient.Add(competitionCoefficient);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, "Erreur lors de la sauvegarde : " + ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateCoefficientCompetitionAsync(CoefficientCompetitionEditDTO dto)
        {
            var cc = await _context.CompetitionCoefficient.FindAsync(dto.Id);
            if (cc == null)
                return (false, "Coefficient introuvable.");

            cc.Coefficient = dto.Coefficient;
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}