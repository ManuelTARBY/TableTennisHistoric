using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;
using static TableTennisHistoric.DTO.ChampionshipDTO;

namespace TableTennisHistoric.Services
{
    public class ChampionshipService: IChampionshipService
    {
        private readonly TableTennisHistoricDbContext _context;
        public ISeasonService _seasonService { get; set; }

        public ChampionshipService(TableTennisHistoricDbContext context, ISeasonService seasonService)
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

        public async Task<(SelectList SeasonSelectList, List<Championship> Championships, List<Club> Clubs)> GetCreatePageDataAsync()
        {
            var seasons = await _context.Season.OrderByDescending(s => s.Name).ToListAsync();
            var seasonSelectList = new SelectList(seasons, "Id", "Name");
            var championships = await _context.Championship.OrderByDescending(c => c.Beginning).ToListAsync();
            var clubs = await _context.Club.OrderBy(c => c.Name).ToListAsync();

            return (seasonSelectList, championships, clubs);
        }

        public async Task CreateChampionshipAsync(Championship championship)
        {
            _context.Championship.Add(championship);
            await _context.SaveChangesAsync();
        }

        public async Task SaveTeamsAndGenerateMatchesAsync(int championshipId, List<ChampionshipTeam> teamLines)
        {
            _context.ChampionshipTeam.AddRange(teamLines);
            await _context.SaveChangesAsync();

            int nbOfTeam = teamLines.Count % 2 == 0 ? teamLines.Count : teamLines.Count + 1;

            var formats = await _context.ChampionshipFormat
                .Where(cf => cf.Nb_of_team == nbOfTeam)
                .OrderBy(cf => cf.Matchday)
                .ToListAsync();

            var matches = new List<ChampionshipMatch>();

            foreach (var format in formats)
            {
                string[] homeList = format.Home.Split(',');
                string[] awayList = format.Away.Split(',');

                for (int i = 0; i < homeList.Length; i++)
                {
                    int homeNumber = int.Parse(homeList[i]);
                    int awayNumber = int.Parse(awayList[i]);

                    var homeTeam = teamLines.First(ct => ct.Number == homeNumber);
                    var awayTeam = teamLines.First(ct => ct.Number == awayNumber);

                    matches.Add(new ChampionshipMatch
                    {
                        ChampionshipId = championshipId,
                        Matchday = format.Matchday,
                        Home_teamId = homeTeam.Id,
                        Away_teamId = awayTeam.Id
                    });
                }
            }

            _context.ChampionshipMatch.AddRange(matches);
            await _context.SaveChangesAsync();
        }

        public async Task<List<object>> GetTeamsByClubAsync(int clubId)
        {
            return await _context.Team
                .Where(t => t.ClubId == clubId)
                .Select(t => (object)new { id = t.Id, name = t.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetChampionshipSelectListAsync()
        {
            var list = await _context.Championship
                .Include(cs => cs.Season)
                .OrderByDescending(cs => cs.Beginning)
                .ThenByDescending(cs => cs.Season.Start_date)
                .ThenByDescending(cs => cs.Phase)
                .ThenBy(cs => cs.Rank)
                .ThenBy(cs => cs.Level)
                .ThenBy(cs => cs.ChampionshipGroup)
                .ToListAsync();

            return list.Select(cs => new SelectListItem
            {
                Value = cs.Id.ToString(),
                Text = $"{cs.Season?.Name ?? "?"} | Phase {cs.Phase} | {Championship.RankLabels.GetValueOrDefault(cs.Rank, cs.Rank.ToString())} {cs.Level} | Poule {cs.ChampionshipGroup}"
            }).ToList();
        }

        public async Task SaveScoresAsync(List<(int Id, int? HomeScore, int? AwayScore)> scores)
        {
            foreach (var (id, homeScore, awayScore) in scores)
            {
                var dbMatch = await _context.ChampionshipMatch.FindAsync(id);
                if (dbMatch != null)
                {
                    dbMatch.Home_score = homeScore;
                    dbMatch.Away_score = awayScore;
                    _context.ChampionshipMatch.Update(dbMatch);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<ChampionshipPageData> GetChampionshipDataAsync(int id)
        {
            var matches = await _context.ChampionshipMatch
                .Where(m => m.ChampionshipId == id)
                .Select(m => new ChampionshipMatchData
                {
                    Id = m.Id,
                    Matchday = m.Matchday,
                    Home_score = m.Home_score,
                    Away_score = m.Away_score,
                    HomeTeamName = _context.ChampionshipTeam
                        .Where(ct => ct.Id == m.Home_teamId)
                        .Select(ct => ct.Team.Name)
                        .FirstOrDefault(),
                    AwayTeamName = _context.ChampionshipTeam
                        .Where(ct => ct.Id == m.Away_teamId)
                        .Select(ct => ct.Team.Name)
                        .FirstOrDefault()
                })
                .OrderBy(m => m.Matchday)
                .ThenBy(m => m.Id)
                .AsNoTracking()
                .ToListAsync();

            var championship = await _context.Championship
                .Include(cs => cs.Season)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            var teamResults = ComputeRanking(matches);

            return new ChampionshipPageData
            {
                Matches = matches,
                Championship = championship,
                TeamResults = teamResults
            };
        }

        private List<ChampionshipTeamResult> ComputeRanking(List<ChampionshipMatchData> matches)
        {
            var teamResults = new List<ChampionshipTeamResult>();

            foreach (var match in matches)
            {
                var homeName = match.HomeTeamName ?? "N/C";
                var awayName = match.AwayTeamName ?? "N/C";

                var home = teamResults.FirstOrDefault(t => t.Name == homeName) ?? new ChampionshipTeamResult { Name = homeName };
                if (!teamResults.Contains(home)) teamResults.Add(home);

                var away = teamResults.FirstOrDefault(t => t.Name == awayName) ?? new ChampionshipTeamResult { Name = awayName };
                if (!teamResults.Contains(away)) teamResults.Add(away);

                if (match.Home_score.HasValue && match.Away_score.HasValue)
                {
                    int hs = match.Home_score.Value;
                    int aScore = match.Away_score.Value;

                    home.NbOfGamePlayed++;
                    home.GameWon = (home.GameWon ?? 0) + hs;
                    home.GameLost = (home.GameLost ?? 0) + aScore;
                    home.GameAverage = (home.GameAverage ?? 0) + (hs - aScore);

                    away.NbOfGamePlayed++;
                    away.GameWon = (away.GameWon ?? 0) + aScore;
                    away.GameLost = (away.GameLost ?? 0) + hs;
                    away.GameAverage = (away.GameAverage ?? 0) + (aScore - hs);

                    if (hs > aScore) { home.Victory++; home.Points += 3; away.Defeat++; away.Points += 1; }
                    else if (hs < aScore) { away.Victory++; away.Points += 3; home.Defeat++; home.Points += 1; }
                    else { home.Draw++; away.Draw++; home.Points += 2; away.Points += 2; }
                }
            }

            return teamResults.OrderByDescending(t => t.Points).ThenByDescending(t => t.GameAverage ?? 0).ToList();
        }
    }
}
