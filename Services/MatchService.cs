using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IPlayerService _playerService { get; set; }
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

        public MatchService(TableTennisHistoricDbContext context, IPlayerService playerService, ICompetitionService competitionService, ISeasonService seasonService, IMatchSetService matchSetService)
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
                    CompetitionSupplement = m.CompetitionSupplement,
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
                CompetitionSupplementName = x.CompetitionSupplement.Name,
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
                    m.CompetitionSupplement,
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
                CompetitionSupplementName = x.CompetitionSupplement == null ? "" : x.CompetitionSupplement.Name,
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
                CompetitionSupplementName = match.CompetitionSupplement.Name,
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
            return ComputeCore(match.Point_difference, match.Result == MatchDTO.MatchResult.V, coefficient);
        }

        public decimal Compute(TableTennisMatch match, CompetitionCoefficient? competitionCoefficient)
        {
            if (competitionCoefficient == null) return 0;
            return ComputeCore(
                match.Opponent_points_at_match - match.My_points_at_match,
                match.Result == TableTennisMatch.MatchResult.V,
                competitionCoefficient.Coefficient);
        }

        public decimal ComputeFromCalculator(decimal myPoints, decimal opponentPoints, decimal coefficient, bool isVictory)
        {
            return ComputeCore(opponentPoints - myPoints, isVictory, coefficient);
        }

        private decimal ComputeCore(decimal pointDifference, bool isVictory, decimal coefficient)
        {
            var table = isVictory ? gainTableVictory : gainTableDefeat;

            foreach (var (minDiff, gain) in table)
            {
                if (pointDifference >= minDiff)
                    return gain * coefficient;
            }

            return 0;
        }

        public async Task<bool> CreateMatchAsync(int competitionCoefficientId, int opponentId, TableTennisMatch match)
        {
            var competitionCoefficient = await _competitionService.GetCompetitionCoefficientByIdAsync(competitionCoefficientId);
            var opponent = await _playerService.GetAllPlayersAsync();
            var player = opponent?.FirstOrDefault(p => p.Id == opponentId);

            if (competitionCoefficient == null || player == null)
                return false;

            match.CompetitionCoefficient = competitionCoefficient;
            match.Opponent = player;

            _context.TableTennisMatch.Add(match);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(List<SelectListItem> CompetitionCoefficients, List<SelectListItem> Opponents)> GetCreateMatchSelectListsAsync()
        {
            var currentSeason = await _seasonService.GetCurrentSeasonAsync();

            var competitionCoefficients = new List<SelectListItem>();

            if (currentSeason != null)
            {
                competitionCoefficients = await _context.CompetitionCoefficient
                    .Include(cc => cc.Competition)
                    .Where(cc => cc.Competition != null && cc.SeasonId == currentSeason.Id)
                    .Select(cc => new SelectListItem
                    {
                        Value = cc.Id.ToString(),
                        Text = cc.Competition.Name
                    })
                    .ToListAsync();
            }

            var opponents = (await _playerService.GetAllPlayersAsync() ?? new List<Player>())
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .OrderBy(o => o.Text)
                .ToList();

            return (competitionCoefficients, opponents);
        }

        public async Task<TableTennisMatch?> GetMatchWithSetsAsync(int id)
        {
            return await _context.TableTennisMatch
                .Include(m => m.Sets)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(List<SelectListItem> CompetitionCoefficients, List<SelectListItem> Opponents, List<SelectListItem> Stages, List<SelectListItem> CompetitionSupplements)> GetUpdateMatchSelectListsAsync(TableTennisMatch match)
        {
            var season = await _context.Season
                .FirstOrDefaultAsync(s => s.Start_date <= match.Date_match && s.End_date >= match.Date_match);

            var competitionCoefficients = new List<SelectListItem>();

            if (season != null)
            {
                competitionCoefficients = await _context.CompetitionCoefficient
                    .Include(cc => cc.Competition)
                    .Where(cc => cc.SeasonId == season.Id)
                    .Select(cc => new SelectListItem
                    {
                        Value = cc.Id.ToString(),
                        Text = cc.Competition.Name
                    })
                    .OrderBy(cc => cc.Text)
                    .ToListAsync();
            }

            var opponents = (await _playerService.GetAllPlayersAsync() ?? new List<Player>())
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .OrderBy(o => o.Text)
                .ToList();

            var stages = await _context.Stage
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();

            var competitionSupplements = await _context.CompetitionSupplement
                .OrderBy(cs => cs.Name)
                .Select(cs => new SelectListItem
                {
                    Value = cs.Id.ToString(),
                    Text = cs.Name
                }).ToListAsync();

            return (competitionCoefficients, opponents, stages, competitionSupplements);
        }

        public List<string> ValidateSets(List<MatchSet> sets, TableTennisMatch.MatchResult result)
        {
            var errors = new List<string>();

            foreach (var set in sets)
            {
                int max = Math.Max(set.Player1Score, set.Player2Score);
                int min = Math.Min(set.Player1Score, set.Player2Score);
                int diff = max - min;

                if (max < 11)
                    errors.Add($"Set {set.SetNumber} : le gagnant doit avoir au moins 11 points ({set.Player1Score}-{set.Player2Score}).");

                if (diff < 2)
                    errors.Add($"Set {set.SetNumber} : l'écart entre les joueurs doit être d'au moins 2 points ({set.Player1Score}-{set.Player2Score}).");
            }

            if (result != TableTennisMatch.MatchResult.F)
            {
                int setsPlayer1 = sets.Count(s => s.Player1Score > s.Player2Score);
                int setsPlayer2 = sets.Count(s => s.Player2Score > s.Player1Score);
                bool player1Won = result == TableTennisMatch.MatchResult.V;

                if (player1Won && setsPlayer1 < setsPlayer2)
                    errors.Add("Le résultat indique une victoire, mais l'adversaire a gagné plus de sets.");

                if (player1Won && setsPlayer1 == setsPlayer2)
                    errors.Add("Le résultat indique une victoire, mais vous avez gagné autant de sets que l'adversaire.");

                if (!player1Won && setsPlayer1 == setsPlayer2)
                    errors.Add("Le résultat indique une défaite, mais vous avez gagné autant de sets que l'adversaire.");

                if (!player1Won && setsPlayer2 < setsPlayer1)
                    errors.Add("Le résultat indique une défaite, mais vous avez gagné plus de sets.");
            }

            return errors;
        }

        public async Task UpdateMatchAsync(TableTennisMatch match, int competitionCoefficientId, int? stageId,
            int? competitionSupplementId, int opponentId, DateTime dateMatch, decimal myPoints, decimal opponentPoints,
            TableTennisMatch.MatchResult result, string? comment, List<MatchSet> sets)
        {
            match.Date_match = DateOnly.FromDateTime(dateMatch);
            match.CompetitionCoefficientId = competitionCoefficientId;
            match.StageId = stageId;
            match.CompetitionSupplementId = competitionSupplementId;
            match.OpponentId = opponentId;
            match.My_points_at_match = myPoints;
            match.Opponent_points_at_match = opponentPoints;
            match.Result = result;
            match.Comment = comment;

            if (sets.Count >= 3)
            {
                _context.MatchSet.RemoveRange(match.Sets ?? Enumerable.Empty<MatchSet>());
                match.Sets = sets;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<MatchesPageDataDTO> GetMatchesPageDataAsync()
        {
            var matchesDTO = await _context.TableTennisMatch
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Competition)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Season)
                .Include(m => m.Opponent)
                    .ThenInclude(p => p.PlayerSeasons)
                        .ThenInclude(pc => pc.Club)
                .Include(m => m.CompetitionSupplement)
                .OrderByDescending(m => m.Date_match)
                .ThenByDescending(m => m.Id)
                .Select(m => new MatchDTO
                {
                    Id = m.Id,
                    Date_of_match = m.Date_match,
                    Competition = m.CompetitionCoefficient.Competition.Name,
                    CompetitionSupplementName = m.CompetitionSupplement == null ? "" : m.CompetitionSupplement.Name,
                    Coefficient = m.CompetitionCoefficient.Coefficient,
                    OpponnentId = m.OpponentId,
                    Opponent_first_name = m.Opponent.First_name,
                    Opponent_last_name = m.Opponent.Last_name,
                    Opponent_full_name = m.Opponent.First_name + " " + m.Opponent.Last_name,
                    Opponent_club = m.Opponent.PlayerSeasons
                        .Where(pc => pc.SeasonId == m.CompetitionCoefficient.SeasonId)
                        .Select(pc => pc.Club.Name)
                        .FirstOrDefault(),
                    Opponent_points_at_match = m.Opponent_points_at_match,
                    Comment = m.Comment,
                    Result = Enum.Parse<MatchDTO.MatchResult>(m.Result.ToString())
                })
                .ToListAsync();

            var currentSeason = await _seasonService.GetCurrentSeasonAsync();

            var competitionCoefficients = currentSeason != null
                ? await _context.CompetitionCoefficient
                    .Include(cc => cc.Competition)
                    .Where(cc => cc.Competition != null && cc.SeasonId == currentSeason.Id)
                    .Select(cc => new SelectListItem
                    {
                        Value = cc.Id.ToString(),
                        Text = cc.Competition.Name
                    })
                    .OrderBy(cc => cc.Text)
                    .ToListAsync()
                : new List<SelectListItem>();

            var opponents = await _context.Player
                .Where(p => p.Id != 1)
                .OrderBy(p => p.First_name)
                .ThenBy(p => p.Last_name)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .ToListAsync();

            var stages = await _context.Stage
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            var competitionsSupplements = await _context.CompetitionSupplement
                .OrderByDescending(s => s.Name)
                .Select(cs => new SelectListItem
                {
                    Value = cs.Id.ToString(),
                    Text = cs.Name
                })
                .ToListAsync();

            return new MatchesPageDataDTO
            {
                MatchesDTO = matchesDTO,
                CompetitionCoefficients = competitionCoefficients,
                Opponents = opponents,
                Stages = stages,
                CompetitionSupplements = competitionsSupplements
            };
        }

        public async Task<bool> CreateMatchWithSetsAsync(CreateMatchDTO dto)
        {
            var competitionCoefficient = await _competitionService.GetCompetitionCoefficientByIdAsync(dto.CompetitionCoefficientId);
            var opponent = await _context.Player.FindAsync(dto.OpponentId);

            if (competitionCoefficient == null || opponent == null)
                return false;

            var match = new TableTennisMatch
            {
                Date_match = DateOnly.FromDateTime(dto.Date_match),
                CompetitionCoefficientId = dto.CompetitionCoefficientId,
                CompetitionSupplementId = dto.CompetitionSupplementId,
                StageId = dto.StageId,
                OpponentId = dto.OpponentId,
                My_points_at_match = dto.My_points_at_match,
                Opponent_points_at_match = dto.Opponent_points_at_match,
                Result = dto.Result,
                CompetitionCoefficient = competitionCoefficient,
                Opponent = opponent,
                Comment = dto.Comment
            };

            _context.TableTennisMatch.Add(match);
            await _context.SaveChangesAsync();

            if (dto.SetMy.Any(s => s.HasValue) || dto.SetOpp.Any(s => s.HasValue))
            {
                for (int i = 0; i < 5; i++)
                {
                    if (dto.SetMy[i].HasValue && dto.SetOpp[i].HasValue)
                    {
                        _context.MatchSet.Add(new MatchSet
                        {
                            MatchId = match.Id,
                            Player1Score = dto.SetMy[i]!.Value,
                            Player2Score = dto.SetOpp[i]!.Value,
                            SetNumber = i + 1,
                            Match = match
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }
        public async Task<List<MatchDTO>> GetMatchesByOpponentAsync(int opponentId)
        {
            return await _context.TableTennisMatch
                .Where(m => m.OpponentId == opponentId)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Competition)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Season)
                .Include(m => m.Opponent)
                    .ThenInclude(p => p.PlayerSeasons)
                        .ThenInclude(pc => pc.Club)
                .Include(m => m.Sets)
                .OrderByDescending(m => m.Date_match)
                .ThenByDescending(m => m.Id)
                .Select(m => new MatchDTO
                {
                    Id = m.Id,
                    Date_of_match = m.Date_match,
                    Competition = m.CompetitionCoefficient.Competition.Name,
                    Coefficient = m.CompetitionCoefficient.Coefficient,
                    Opponent_first_name = m.Opponent.First_name,
                    Opponent_last_name = m.Opponent.Last_name,
                    Opponent_club = m.Opponent.PlayerSeasons
                        .Where(pc => pc.SeasonId == m.CompetitionCoefficient.SeasonId)
                        .Select(pc => pc.Club.Name)
                        .FirstOrDefault(),
                    Season_name = m.CompetitionCoefficient.Season.Name,
                    My_points_at_match = m.My_points_at_match,
                    Opponent_points_at_match = m.Opponent_points_at_match,
                    Point_difference = m.Opponent_points_at_match - m.My_points_at_match,
                    Comment = m.Comment,
                    Result = m.Result == TableTennisMatch.MatchResult.V
                        ? MatchDTO.MatchResult.V
                        : m.Result == TableTennisMatch.MatchResult.D
                            ? MatchDTO.MatchResult.D
                            : MatchDTO.MatchResult.F,
                    MatchSets = m.Sets
                        .OrderBy(s => s.SetNumber)
                        .Select(s => new SetDTO
                        {
                            SetNumber = s.SetNumber,
                            Player1Score = s.Player1Score,
                            Player2Score = s.Player2Score
                        })
                        .ToList()
                })
                .ToListAsync();
        }
        public async Task DeleteMatchAsync(int matchId)
        {
            var match = await _context.TableTennisMatch.FindAsync(matchId);
            if (match != null)
            {
                _context.TableTennisMatch.Remove(match);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<MatchDTO>> GetFilteredMatchesAsync(MatchFilterDTO filter)
        {
            var query = _context.TableTennisMatch
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Competition)
                .Include(m => m.CompetitionCoefficient)
                    .ThenInclude(cc => cc.Season)
                .Include(m => m.Opponent)
                    .ThenInclude(p => p.PlayerSeasons)
                        .ThenInclude(ps => ps.Club)
                .Include(m => m.Sets)
                .AsQueryable();

            // Filtre saison
            if (filter.SeasonId.HasValue)
                query = query.Where(m => m.CompetitionCoefficient.SeasonId == filter.SeasonId);

            // Filtre compétition
            if (filter.CompetitionId.HasValue)
            {
                if (filter.CompetitionId.Value == -1)
                {
                    query = query.Where(m => m.CompetitionCoefficient.Competition.Name.Contains("Tournoi"));
                }
                else if (filter.CompetitionId.Value == -2)
                {
                    query = query.Where(m => m.CompetitionCoefficient.Competition.Name.Contains("Critérium"));
                }
                else
                {
                    query = query.Where(m => m.CompetitionCoefficient.CompetitionId == filter.CompetitionId);
                }
            }

            // Filtre club adversaire
            if (filter.ClubId.HasValue)
                query = query.Where(m => m.Opponent.PlayerSeasons
                    .Any(ps => ps.ClubId == filter.ClubId
                            && ps.SeasonId == m.CompetitionCoefficient.SeasonId));

            // Filtre période
            if (filter.DateFrom.HasValue)
                query = query.Where(m => m.Date_match >= filter.DateFrom);

            if (filter.DateTo.HasValue)
                query = query.Where(m => m.Date_match <= filter.DateTo);

            // Filtre résultat
            if (filter.Result.HasValue)
                query = query.Where(m => m.Result == filter.Result);

            // Filtre nombre de sets
            if (filter.NbOfSets.HasValue)
                query = query.Where(m => m.Sets.Count == filter.NbOfSets);

            // Filtres classement adversaire
            if (filter.OpponentPointsMin.HasValue)
                query = query.Where(m => m.Opponent_points_at_match >= filter.OpponentPointsMin);

            if (filter.OpponentPointsMax.HasValue)
                query = query.Where(m => m.Opponent_points_at_match <= filter.OpponentPointsMax);


            switch (filter.Perf)
            {
                case "Perf":
                    query = query
                        .Where(m => (m.Opponent_points_at_match - m.My_points_at_match) >= 25m
                                 && m.Result == TableTennisMatch.MatchResult.V)
                        .OrderByDescending(m => m.Opponent_points_at_match - m.My_points_at_match);
                    break;

                case "UnderPerf":
                    query = query
                        .Where(m => (m.My_points_at_match - m.Opponent_points_at_match) >= 25m
                                 && m.Result == TableTennisMatch.MatchResult.D)
                        .OrderByDescending(m => m.My_points_at_match - m.Opponent_points_at_match);
                    break;
            }

            List<TableTennisMatch> raw;

            if (filter.Perf == "Perf" || filter.Perf == "UnderPerf")
            {
               raw = await query
                    .AsNoTracking()
                    .ToListAsync();
            }
            else
            {
                raw = await query
                    .OrderByDescending(m => m.Date_match)
                    .ThenByDescending(m => m.Id)
                    .AsNoTracking()
                    .ToListAsync();
            }

            return raw.Select(m => new MatchDTO
            {
                Id = m.Id,
                Date_of_match = m.Date_match,
                Competition = m.CompetitionCoefficient.Competition.Name,
                Coefficient = m.CompetitionCoefficient.Coefficient,
                Season_name = m.CompetitionCoefficient.Season.Name,
                OpponnentId = m.OpponentId,
                Opponent_first_name = m.Opponent.First_name,
                Opponent_last_name = m.Opponent.Last_name,
                Opponent_full_name = m.Opponent.First_name + " " + m.Opponent.Last_name,
                Opponent_club = m.Opponent.PlayerSeasons
                    .Where(ps => ps.SeasonId == m.CompetitionCoefficient.SeasonId)
                    .Select(ps => ps.Club.Name_abrev)
                    .FirstOrDefault() ?? "",
                My_points_at_match = m.My_points_at_match,
                Opponent_points_at_match = m.Opponent_points_at_match,
                Point_difference = m.Opponent_points_at_match - m.My_points_at_match,
                Comment = m.Comment,
                Result = (MatchDTO.MatchResult)m.Result,
                Gain = ComputeCore(
                    m.Opponent_points_at_match - m.My_points_at_match,
                    m.Result == TableTennisMatch.MatchResult.V,
                    m.CompetitionCoefficient.Coefficient),
                MatchSets = m.Sets.OrderBy(s => s.SetNumber).Select(s => new SetDTO
                {
                    SetNumber = s.SetNumber,
                    Player1Score = s.Player1Score,
                    Player2Score = s.Player2Score
                }).ToList()
            }).ToList();
        }
    }
}
