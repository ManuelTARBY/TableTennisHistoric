using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class ClubService
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
            ClubDTO clubDTO = new()
            {
                License_number = club.License_number,
                Name = club.Name,
                Name_abrev = club.Name_abrev,
                City = club.City
            };

            return clubDTO;
        }
    }
}
