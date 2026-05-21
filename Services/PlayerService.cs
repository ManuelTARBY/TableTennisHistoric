using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class PlayerService: IPlayerService
    {
        private readonly TableTennisHistoricDbContext _context;
        public ISeasonService _season;

        public PlayerService(TableTennisHistoricDbContext context, ISeasonService season)
        {
            _context = context;
            _season = season;
        }

        public PlayerDTO? ConvertPlayerToPlayerDTO(Player player)
        {
            if (player == null) { return null; }
            
            PlayerDTO playerDTO = new PlayerDTO
            {
                License_number = player.License_number,
                First_name = player.First_name,
                Last_name = player.Last_name,
                Is_me = player.Is_me
            };

            return playerDTO;
        }

        public async Task<decimal?> GetPointsBeginningOfCurrentSeasonAsync(Player player)
        {
            Season? currentSeason = await _season.GetCurrentSeasonAsync();

            if (currentSeason == null) { return null; }

            return await GetPointsBeginningOfSeasonAsync(player, currentSeason);
        }

        public async Task<decimal?> GetPointsMiddleOfCurrentSeasonAsync(Player player)
        {
            Season? currentSeason = await _season.GetCurrentSeasonAsync();

            if (currentSeason == null) { return null; }
            
            return await GetPointsMiddleOfSeasonAsync(player, currentSeason);
        }

        public async Task<decimal?> GetPointsBeginningOfSeasonAsync(Player player, Season season)
        {
            PlayerSeason? playerSeason = await _context.PlayerSeason.FirstOrDefaultAsync(sp => sp.PlayerId == player.Id && sp.SeasonId == season.Id);
            return playerSeason == null ? null : playerSeason.Points_start;
        }

        public async Task<decimal?> GetPointsMiddleOfSeasonAsync(Player player, Season season)
        {
            PlayerSeason? playerSeason = await _context.PlayerSeason.FirstOrDefaultAsync(sp => sp.PlayerId == player.Id && sp.SeasonId == season.Id);
            return playerSeason == null ? null : playerSeason.Points_middle;
        }

        public async Task<Player?> GetMeAsync()
        {
            return await _context.Player.FirstOrDefaultAsync(p => p.Is_me);
        }

        public async Task<List<Player>?> GetAllPlayersAsync()
        {
            return await _context.Player.ToListAsync();
        }

        public async Task<Club?> GetClubByPlayerIdAndSeasonIdAsync(int playerId, int seasonId)
        {
            PlayerSeason? playerClub = await _context.PlayerSeason.Where(pc => pc.PlayerId == playerId && pc.SeasonId == seasonId).FirstOrDefaultAsync();
            
            if (playerClub == null) { return null; }

            Club? club = await _context.Club.FirstOrDefaultAsync(c => c.Id == playerClub.ClubId);

            return club;
        }

        public async Task<Club?> GetClubByPlayerAndSeasonAsync(Player Player, Season Season)
        {
            if (Player == null || Season == null) { return null; }

            PlayerSeason? playerClub = await _context.PlayerSeason.Where(pc => pc.PlayerId == Player.Id && pc.SeasonId == Season.Id).FirstOrDefaultAsync();

            if (playerClub == null) { return null; }

            Club? club = await _context.Club.FirstOrDefaultAsync(c => c.Id == playerClub.ClubId);

            return club;
        }

        public async Task<Club?> GetCurrentClubAsync(Player player)
        {
            SeasonService seasonService = new SeasonService(_context);
            Season? currentSeason = await seasonService.GetCurrentSeasonAsync();
            return await GetClubByPlayerAndSeasonAsync(player, currentSeason);
        }

        public async Task<PlayerSeason?> GetPlayerSeasonByPlayerIdAndSeasonIdAsync(int playerId, int seasonId)
        {
            return await _context.PlayerSeason.Where(ps => ps.PlayerId == playerId && ps.SeasonId == seasonId).FirstOrDefaultAsync();
        }

        public async Task<PlayerSeason> GetPlayerSeasonByPlayerAndSeasonAsync(Player Player, Season Season)
        {
            return await _context.PlayerSeason.Where(ps => ps.PlayerId == Player.Id && ps.SeasonId == Season.Id).FirstOrDefaultAsync(); ;
        }

        public async Task<PlayerSeason?> GetPlayerSeasonAsync(Player player, Season season)
        {
            if (player == null || season == null)
                return null;

            // Recherche le PlayerSeason correspondant au joueur et à la saison
            return await _context.PlayerSeason
                .AsNoTracking() // optimisation lecture seule
                .FirstOrDefaultAsync(ps => ps.PlayerId == player.Id && ps.SeasonId == season.Id);
        }

    }
}
