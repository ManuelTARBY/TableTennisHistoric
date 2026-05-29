using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class ClubService : IClubService
    {
        private readonly TableTennisHistoricDbContext _context;

        public ClubService(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public async Task<Club?> GetClubByIdAsync(int id)
        {
            return await _context.Club.FirstOrDefaultAsync(c => c.Id == id);
        }

        public ClubDTO ConvertClubToClubDTO(Club club)
        {
            return new ClubDTO
            {
                License_number = club.License_number,
                Name = club.Name,
                Name_abrev = club.Name_abrev,
                City = club.City
            };
        }

        public async Task<ClubsPageDataDTO> GetClubsPageDataAsync(int? selectedClubId)
        {
            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();
            var clubSelectList = new SelectList(clubs, "Id", "Name");

            var selectedClubTeams = new List<Team>();
            if (selectedClubId.HasValue)
            {
                selectedClubTeams = await _context.Team
                    .Where(t => t.ClubId == selectedClubId.Value)
                    .OrderBy(t => t.Id)
                    .ToListAsync();
            }

            return new ClubsPageDataDTO
            {
                Clubs = clubs,
                ClubSelectList = clubSelectList,
                SelectedClubTeams = selectedClubTeams
            };
        }

        public async Task CreateClubAsync(Club club)
        {
            _context.Club.Add(club);
            await _context.SaveChangesAsync();
        }

        public async Task CreateTeamsAsync(int clubId, string baseName, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                _context.Team.Add(new Team
                {
                    Name = $"{baseName} {i}",
                    ClubId = clubId
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string? Error)> UpdateClubAsync(int id, Club updated)
        {
            var club = await _context.Club.FindAsync(id);
            if (club == null)
                return (false, "Club introuvable.");

            club.Name = updated.Name;
            club.Name_abrev = updated.Name_abrev;
            club.License_number = updated.License_number;
            club.City = updated.City;
            club.Department = updated.Department;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteClubAsync(int id)
        {
            var club = await _context.Club
                .Include(c => c.Teams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
                return (false, "Club introuvable.");

            // Suppression manuelle des PlayerSeason liés
            var playerSeasons = await _context.PlayerSeason
                .Where(ps => ps.ClubId == id)
                .ToListAsync();
            _context.PlayerSeason.RemoveRange(playerSeasons);

            // Suppression manuelle des équipes
            _context.Team.RemoveRange(club.Teams);

            // Suppression du club
            _context.Club.Remove(club);

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}