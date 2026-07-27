using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly TableTennisHistoricDbContext _context;
        public ISeasonService _season;

        public PlayerService(TableTennisHistoricDbContext context, ISeasonService season)
        {
            _context = context;
            _season = season;
        }

        //
        public async Task<(bool Success, string? Error, int? PlayerId)> CreatePlayerWithAffiliationAsync(Player player, PlayerSeason playerSeason)
        {
            // Vérifie si le joueur existe déjà
            var existingPlayer = await _context.Player
                .FirstOrDefaultAsync(p =>
                    p.First_name.ToLower() == player.First_name.ToLower() &&
                    p.Last_name.ToLower() == player.Last_name.ToLower());

            if (existingPlayer == null)
            {
                // Crée le joueur
                player.Last_name = player.Last_name.ToUpper();
                _context.Player.Add(player);
                await _context.SaveChangesAsync();
                existingPlayer = player;
            }

            // Vérifie si l'affiliation existe déjà
            var existingAffiliation = await _context.PlayerSeason
                .AnyAsync(ps => ps.PlayerId == existingPlayer.Id && ps.SeasonId == playerSeason.SeasonId);

            if (existingAffiliation)
                return (false, $"Une affiliation existe déjà pour {existingPlayer.First_name} {existingPlayer.Last_name} sur cette saison.", existingPlayer.Id);

            // Crée l'affiliation
            playerSeason.PlayerId = existingPlayer.Id;
            _context.PlayerSeason.Add(playerSeason);
            await _context.SaveChangesAsync();

            return (true, null, existingPlayer.Id);
        }

        //

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

        public async Task<List<PlayerWithClubDTO>> GetPlayersWithClubBySeasonAsync(Season season)
        {
            return await _context.Player
                .Include(p => p.PlayerSeasons)
                    .ThenInclude(pc => pc.Club)
                .Where(p => p.PlayerSeasons.Any(pc => pc.SeasonId == season.Id))
                .Select(p => new PlayerWithClubDTO
                {
                    Player = p,
                    Club = p.PlayerSeasons
                        .Where(pc => pc.SeasonId == season.Id)
                        .Select(pc => pc.Club)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<RankingHistoryDTO> GetRankingHistoryByPlayerIdAsync(int? playerId)
        {
            var playerSeasons = await _context.PlayerSeason
                .Where(ps => ps.PlayerId == playerId)
                .Include(ps => ps.Season)
                .OrderBy(ps => ps.Season.Start_date)
                .ToListAsync();

            var rankingHistory = new Dictionary<DateOnly, decimal?>();

            foreach (var playerSeason in playerSeasons)
            {
                rankingHistory.Add(playerSeason.Season.Start_date, playerSeason.Points_start ?? 0);

                if (DateOnly.FromDateTime(DateTime.Now) > playerSeason.Season.Start_date.AddMonths(4))
                {
                    DateOnly P2Date = new DateOnly(playerSeason.Season.End_date.Year, 01, 01);
                    rankingHistory.Add(P2Date, playerSeason.Points_middle ?? 0);
                }
            }

            int maxValue = (int)rankingHistory.Values
                .OfType<decimal>()
                .DefaultIfEmpty(500m)
                .Max();

            maxValue = (int)(Math.Ceiling(maxValue / 10m) * 10m) + 20;

            return new RankingHistoryDTO
            {
                RankingHistory = rankingHistory,
                RankingMaxValue = maxValue
            };
        }

        public async Task<SelectList> GetOpponentsSelectListAsync()
        {
            var opponents = await _context.Player
                .Where(p => p.Is_me == false)
                .OrderBy(p => p.First_name)
                .ThenBy(p => p.Last_name)
                .Select(p => new
                {
                    Id = p.Id,
                    FullName = $"{p.First_name} {p.Last_name}"
                })
                .ToListAsync();

            return new SelectList(opponents, "Id", "FullName");
        }

        public async Task<(bool Success, string? Error)> CreatePlayerSeasonAsync(PlayerSeason playerSeason)
        {
            var exists = await _context.PlayerSeason
                .AnyAsync(ps => ps.PlayerId == playerSeason.PlayerId && ps.SeasonId == playerSeason.SeasonId);

            if (exists)
                return (false, "Une affiliation existe déjà pour ce joueur et cette saison.");

            _context.PlayerSeason.Add(playerSeason);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task CreatePlayerAsync(Player player)
        {
            player.Last_name = player.Last_name.ToUpper();
            _context.Player.Add(player);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlayerSeasonAsync(PlayerSeasonDTO dto)
        {
            var playerSeason = await _context.PlayerSeason.FindAsync(dto.Id);
            if (playerSeason == null) return;

            playerSeason.Points_start = dto.Points_start;
            playerSeason.Points_middle = dto.Points_middle;
            playerSeason.ClubId = dto.ClubId;
            playerSeason.Category = dto.Category;

            await _context.SaveChangesAsync();
        }

        public async Task DeletePlayerSeasonAsync(int id)
        {
            var playerSeason = await _context.PlayerSeason.FindAsync(id);
            if (playerSeason != null)
            {
                _context.PlayerSeason.Remove(playerSeason);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PlayerSeasonDTO>> GetPlayerSeasonsByPlayerIdAsync(int playerId)
        {
            return await _context.PlayerSeason
                .Include(ps => ps.Player)
                .Include(ps => ps.Season)
                .Include(ps => ps.Club)
                .Where(ps => ps.PlayerId == playerId)
                .OrderBy(ps => ps.Season.Start_date)
                .Select(ps => new PlayerSeasonDTO
                {
                    Id = ps.Id,
                    PlayerFullName = ps.Player.First_name + " " + ps.Player.Last_name,
                    SeasonName = ps.Season.Name,
                    ClubName = ps.Club.Name,
                    Points_start = ps.Points_start,
                    Points_middle = ps.Points_middle,
                    PlayerId = ps.PlayerId,
                    SeasonId = ps.SeasonId,
                    ClubId = ps.ClubId,
                    Category = ps.Category
                })
                .ToListAsync();
        }

        public async Task<(SelectList Players, SelectList Clubs, SelectList Seasons)> GetCreatePlayerSelectListsAsync()
        {
            var players = new SelectList(
                await _context.Player
                    .OrderBy(p => p.First_name)
                    .Select(p => new { p.Id, FullName = p.First_name + " " + p.Last_name })
                    .ToListAsync(),
                "Id", "FullName");

            var clubs = new SelectList(
                await _context.Club.OrderBy(c => c.Name).ToListAsync(),
                "Id", "Name");

            var seasons = new SelectList(
                await _context.Season.OrderByDescending(s => s.Start_date).ToListAsync(),
                "Id", "Name");

            return (players, clubs, seasons);
        }

        public async Task<List<PlayerEditDTO>> GetAllPlayerEditDTOAsync()
        {
            return await _context.Player
                .OrderBy(p => p.First_name)
                .ThenBy(p => p.Last_name)
                .Select(p => new PlayerEditDTO
                {
                    Id = p.Id,
                    First_name = p.First_name,
                    Last_name = p.Last_name,
                    License_number = p.License_number
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string? Error)> UpdatePlayerAsync(PlayerEditDTO dto)
        {
            var player = await _context.Player.FindAsync(dto.Id);
            if (player == null)
                return (false, "Joueur introuvable.");

            player.First_name = dto.First_name;
            player.Last_name = dto.Last_name.ToUpper();
            player.License_number = dto.License_number;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<PlayerDTO?> GetPlayerByLicenseNumberAsync(string licensenumber)
        {
            var player = await _context.Player.FirstOrDefaultAsync(p => p.License_number == licensenumber);
            if (player != null)
            {
                return ConvertPlayerToPlayerDTO(player);
            }
            else
            {
                return null;
            }
        }
    }
}
