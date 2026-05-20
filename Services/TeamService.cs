using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Services
{
    public class TeamService
    {
        private readonly TableTennisHistoricDbContext _context;
        public ClubService _clubService { get; set; }

        public TeamService(TableTennisHistoricDbContext context, ClubService clubService)
        {
            _context = context;
            _clubService = clubService;
        }

        public async Task<Team?> GetTeamByIdSync(int id)
        {
            return await _context.Team.FindAsync(id);
        }

        public TeamDTO ConvertToToTeamDTO(Team team)
        {
            Club? club = _context.Club.Find(team.ClubId);

            return new TeamDTO {
                Name = team.Name,
                ClubDTO = club != null ? _clubService.ConvertClubToClubDTO(club) : null,
            };
        }
    }
}
