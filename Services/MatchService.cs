using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class MatchService: IMatchService
    {
        private readonly TableTennisHistoricDbContext _context;
        public PlayerService _playerService { get; set; }
        public ICompetitionService _competitionService { get; set; }
        public ISeasonService _seasonService { get; set; }
        public IMatchSetService _matchSetService { get; set; }
        private static readonly (decimal minDiff, decimal gain)[] gainTableVictory =
            {
                (500, 40),
                (400, 28),
                (300, 22),
                (200, 17),
                (150, 13),
                (100, 10),
                (50, 8),
                (25, 7),
                (-24.99m, 6),
                (-49.99m, 5.5m),
                (-99.99m, 5),
                (-149.99m, 4),
                (-199.99m, 3),
                (-299.99m, 2),
                (-399.99m, 1),
                (-499.99m, 0.5m),
                (-10000, 0)
            };
        private static readonly (decimal minDiff, decimal gain)[] gainTableDefeat =
            {
                (400, 0),
                (300, -0.5m),
                (200, -1),
                (150, -2),
                (100, -3),
                (50, -4),
                (25, -4.5m),
                (-24.99m, -5),
                (-49.99m, -6),
                (-99.99m, -7),
                (-149.99m, -8),
                (-199.99m, -10),
                (-299.99m, -12.5m),
                (-399.99m, -16),
                (-499.99m, -20),
                (-10000, -29)
            };

        public MatchService(TableTennisHistoricDbContext context, PlayerService playerService, ICompetitionService competitionService, ISeasonService seasonService, IMatchSetService matchSetService)
        {
            _context = context;
            _playerService = playerService;
            _competitionService = competitionService;
            _seasonService = seasonService;
            _matchSetService = matchSetService;
        }

        public async Task<List<TableTennisMatch>?> GetAllMatchesAsync()
        {
            return await _context.TableTennisMatch.ToListAsync();
        }

        public async Task<List<TableTennisMatch>?> GetAllMatchesByDateDescendingAsync()
        {
            return await _context.TableTennisMatch
                .OrderByDescending(m => m.Date_match)
                .ThenBy(m => m.Id)
                .ToListAsync();
        }

        public async Task<List<MatchDTO>> GetAllMatchesDTOBySeasonAsync(Season season)
        {
            if (season == null) return new List<MatchDTO>();

            // Projection légère : on ne récupère que les champs utiles
            var raw = await _context.TableTennisMatch
                .Where(m => m.CompetitionCoefficient.SeasonId == season.Id)
                .Select(m => new
                {
                    m.Id,
                    m.Date_match,
                    CompetitionName = m.CompetitionCoefficient.Competition.Name,
                    Stage = m.Stage.Name,
                    CompetitionCoefficientValue = m.CompetitionCoefficient.Coefficient,
                    OpponentId = m.OpponentId,
                    OpponentFirstName = m.Opponent.First_name,
                    OpponentLastName = m.Opponent.Last_name,
                    OpponentPoints = m.Opponent_points_at_match,
                    MyPoints = m.My_points_at_match,
                    Point_Difference = m.Opponent_points_at_match - m.My_points_at_match,
                    Result = m.Result,
                    Comment = m.Comment,
                    Sets = m.Sets.Select(s => new { s.SetNumber, s.Player1Score, s.Player2Score }).ToList(),

                    // Sous-requête pour récupérer le club du joueur pour la saison demandée
                    OpponentClubAbrev = _context.PlayerSeason
                        .Where(pc => pc.PlayerId == m.OpponentId && pc.SeasonId == season.Id)
                        .Select(pc => pc.Club.Name_abrev)
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .OrderBy(x => x.Date_match)
                .ThenBy(x => x.Id)
                .ToListAsync();

            // Convertit en MatchDTO et calcule Gain avec le service (Compute ne peut pas être exécuté dans la projection SQL)
            var matchesDto = raw.Select(x => new MatchDTO
            {
                Date_of_match = x.Date_match,
                Competition = x.CompetitionName,
                Coefficient = x.CompetitionCoefficientValue,
                Stage_name = x.Stage,
                OpponnentId = x.OpponentId,
                Opponent_first_name = x.OpponentFirstName,
                Opponent_last_name = x.OpponentLastName,
                Opponent_full_name = x.OpponentFirstName + " " + x.OpponentLastName,
                Opponent_club = x.OpponentClubAbrev ?? "",
                Opponent_points_at_match = x.OpponentPoints,
                My_points_at_match = x.MyPoints,
                Point_difference = x.Point_Difference,
                Comment = x.Comment,
                Result = (MatchDTO.MatchResult)x.Result,
                MatchSets = x.Sets.Select(s => new SetDTO
                {
                    SetNumber = s.SetNumber,
                    Player1Score = s.Player1Score,
                    Player2Score = s.Player2Score
                }).ToList()
            }).ToList();

            foreach (var dto in matchesDto)
            {
                dto.Gain = ComputeDTO(dto, dto.Coefficient);
            }

            return matchesDto;
        }

        public async Task<List<MatchDTO>> GetAllMatchesDTOAsync()
        {
            // Projection légère : on ne récupère que les champs utiles
            var raw = await _context.TableTennisMatch
                .Select(m => new
                {
                    m.Id,
                    m.Date_match,
                    m.CompetitionCoefficient,
                    CompetitionName = m.CompetitionCoefficient.Competition.Name,
                    CompetitionCoefficientValue = m.CompetitionCoefficient.Coefficient,
                    m.CompetitionCoefficient.SeasonId,
                    SeasonName = m.CompetitionCoefficient.Season.Name,
                    m.Stage.Name,
                    m.OpponentId,
                    OpponentFirstName = m.Opponent.First_name,
                    OpponentLastName = m.Opponent.Last_name,

                    OpponentPoints = m.Opponent_points_at_match,
                    MyPoints = m.My_points_at_match,
                    Point_Difference = m.Opponent_points_at_match - m.My_points_at_match,
                    m.Result,
                    m.Comment,

                    Sets = m.Sets
                        .Select(s => new { s.SetNumber, s.Player1Score, s.Player2Score })
                        .ToList(),

                    OpponentClubAbrev = m.Opponent.PlayerSeasons
                        .OrderByDescending(ps => ps.Season.Start_date)
                        .Select(ps => ps.Club.Name_abrev)
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .OrderBy(x => x.Date_match)
                .ThenBy(x => x.Id)
                .ToListAsync();


            // Convertit en MatchDTO et calcule Gain avec ton service (Compute ne peut pas être exécuté dans la projection SQL)
            var matchesDto = raw.Select(x => new MatchDTO
            {
                Id = x.Id,
                Date_of_match = x.Date_match,
                CompetitionCoefficient = x.CompetitionCoefficient,
                Competition = x.CompetitionName,
                Stage_name = x.Name,
                Coefficient = x.CompetitionCoefficientValue,
                SeasonId = x.SeasonId,
                Season_name = x.SeasonName,
                OpponnentId = x.OpponentId,
                Opponent_first_name = x.OpponentFirstName,
                Opponent_last_name = x.OpponentLastName,
                Opponent_full_name = x.OpponentFirstName + " " + x.OpponentLastName,
                Opponent_club = x.OpponentClubAbrev ?? "",
                Opponent_points_at_match = x.OpponentPoints,
                My_points_at_match = x.MyPoints,
                Point_difference = x.Point_Difference,
                Comment = x.Comment,
                Result = (MatchDTO.MatchResult)x.Result,
                MatchSets = x.Sets.Select(s => new SetDTO
                {
                    SetNumber = s.SetNumber,
                    Player1Score = s.Player1Score,
                    Player2Score = s.Player2Score
                }).ToList()
            }).ToList();

            foreach (var dto in matchesDto)
            {
                dto.Gain = ComputeDTO(dto, dto.Coefficient);
            }

            return matchesDto;
        }

        public async Task<MatchDTO> ConvertMatchToMatchDTOAsync(TableTennisMatch match)
        {
            // Récupère le joueur adverse
            Player? Opponent = await _context.Player.FirstOrDefaultAsync(p => p.Id == match.OpponentId);
            PlayerDTO? OpponentDTO = new PlayerDTO();
            if (Opponent != null)
            {
                OpponentDTO = _playerService.ConvertPlayerToPlayerDTO(Opponent);
            }

            // Récupère le coefficient
            CompetitionCoefficient? competitionCoefficient = await _competitionService.GetCompetitionCoefficientByIdAsync(match.CompetitionCoefficientId);

            // Récupère la compétition
            Competition? competition = await _competitionService.GetCompetitionByCompetitionCoefficientIdAsync(match.CompetitionCoefficientId);
            CompetitionDTO? competitionDTO = _competitionService.ConvertCompetitionToCompetitionDTO(competition);

            // Récupère le club
            Club? Club = new Club();
            if (competition != null)
            {
                Season? Season = await _seasonService.GetSeasonByDateAsync(match.Date_match);
                Club = await _playerService.GetClubByPlayerAndSeasonAsync(Opponent, Season);
            }

            // Récupère les sets du match
            List<MatchSet>? matchSets = await GetMatchSetsAsync(match);
            List<SetDTO>? matchSetDTOs = matchSets == null ? null : ConvertMatchSetsToMatchSetDTOs(matchSets);

            MatchDTO matchDTO = new MatchDTO
            {
                Id = match.Id,
                Date_of_match = match.Date_match,
                Competition = competition.Name,
                Coefficient = competitionCoefficient != null ? competitionCoefficient.Coefficient : 0,
                Stage_name = match.Stage != null ? match.Stage.Name : "",
                Opponent_first_name = Opponent != null ? OpponentDTO.First_name : "",
                Opponent_last_name = Opponent != null ? OpponentDTO.Last_name : "",
                Opponent_club = Club != null ? (Club.Name_abrev != "" ? Club.Name_abrev : Club.Name) : "",
                My_points_at_match = match.My_points_at_match,
                Opponent_points_at_match = match.Opponent_points_at_match,
                Point_difference = match.Opponent_points_at_match - match.My_points_at_match,
                Comment = match.Comment,
                Result = (MatchDTO.MatchResult)match.Result,
                MatchSets = matchSetDTOs,
                Gain = Compute(match, competitionCoefficient)
            };

            return matchDTO;
        }

        public void DetermineTypeOfResult(MatchDTO match, ref Dictionary<string, int> DetailedResults)
        {
            switch (match.Result)
            {
                case (MatchDTO.MatchResult)TableTennisMatch.MatchResult.V:
                    if (match.Opponent_points_at_match - match.My_points_at_match >= 25)
                    {
                        DetailedResults["Perfs"]++;
                    }
                    else
                    {
                        DetailedResults["Victoires normales"]++;
                    }
                    break;
                case (MatchDTO.MatchResult)TableTennisMatch.MatchResult.D:
                    if (match.My_points_at_match - match.Opponent_points_at_match >= 25)
                    {
                        DetailedResults["Contre-perfs"]++;
                    }
                    else
                    {
                        DetailedResults["Défaites normales"]++;
                    }
                    break;
                case (MatchDTO.MatchResult)TableTennisMatch.MatchResult.F:
                    DetailedResults["Forfaits"]++;
                    break;
                default:
                    break;
            }
        }

        public List<SetDTO> ConvertMatchSetsToMatchSetDTOs(List<MatchSet> matchSets)
        {
            List<SetDTO> setDTOs = new List<SetDTO>();

            foreach (MatchSet matchSet in matchSets)
            {
                SetDTO setDTO = new()
                {
                    Player1Score = matchSet.Player1Score,
                    Player2Score = matchSet.Player2Score,
                };
                setDTOs.Add(setDTO);
            }

            return setDTOs;
        }

        public async Task<List<MatchSet>?> GetMatchSetsAsync(TableTennisMatch match)
        {
            return await _context.MatchSet.Where(ms => ms.MatchId == match.Id).ToListAsync();
        }

        public decimal ComputeDTO(MatchDTO match, decimal coefficient)
        {

            if (coefficient == null) { return 0; }

            var table = match.Result switch
            {
                MatchDTO.MatchResult.V => gainTableVictory,
                MatchDTO.MatchResult.D => gainTableDefeat,
                _ => Array.Empty<(decimal, decimal)>()
            };

            foreach (var (minDiff, gain) in table)
            {
                if (match.Point_difference >= minDiff)
                    return gain * coefficient;
            }

            return 0;
        }

        public decimal Compute(TableTennisMatch match, CompetitionCoefficient? competitionCoefficient)
        {

            if (competitionCoefficient == null) { return 0; }

            decimal diff = match.Opponent_points_at_match - match.My_points_at_match;

            var table = match.Result switch
            {
                TableTennisMatch.MatchResult.V => gainTableVictory,
                TableTennisMatch.MatchResult.D => gainTableDefeat,
                _ => Array.Empty<(decimal, decimal)>()
            };

            foreach (var (minDiff, gain) in table)
            {
                if (diff >= minDiff)
                    return gain * competitionCoefficient.Coefficient;
            }

            return 0;
        }
    }
}
