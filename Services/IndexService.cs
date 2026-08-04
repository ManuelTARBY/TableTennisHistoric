using System.Globalization;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Services
{
    public class IndexService : IIndexService
    {
        private readonly TableTennisHistoricDbContext _context;
        private readonly IMatchService _matchService;
        private readonly IPlayerService _playerService;
        private readonly ISeasonService _seasonService;

        public IndexService(TableTennisHistoricDbContext context, IMatchService matchService,
            IPlayerService playerService, ISeasonService seasonService)
        {
            _context = context;
            _matchService = matchService;
            _playerService = playerService;
            _seasonService = seasonService;
        }

        public async Task<IndexDataDTO> GetIndexDataAsync(int seasonId)
        {
            var season = await _seasonService.GetSeasonByIdAsync(seasonId);
            var data = new IndexDataDTO();

            var allMatchesDTO = await _matchService.GetAllMatchesDTOAsync();
            data.MatchesDTO = allMatchesDTO.Where(m => m.SeasonId == seasonId).ToList();

            var playerMe = await _playerService.GetMeAsync();
            data.PlayerMe = playerMe;

            if (playerMe != null)
            {
                data.PlayerSeason = await _playerService.GetPlayerSeasonByPlayerAndSeasonAsync(playerMe, season);
                data.PointsBeginningSeason = data.PlayerSeason?.Points_start;
            }

            data.MonthlyPoints = data.PointsBeginningSeason ?? 0;

            for (int i = 0; i < 12; i++)
            {
                data.MonthliesPointsOrdered.Add(season.Start_date.AddMonths(i), i == 0 ? data.PointsBeginningSeason : 0);
            }

            var detailedResults = new Dictionary<string, int>
            {
                { "Perfs", 0 }, { "Victoires normales", 0 },
                { "Défaites normales", 0 }, { "Contre-perfs", 0 }, { "Forfaits", 0 }
            };

            var victoryDistribution = new Dictionary<int, int>();
            var defeatDistribution = new Dictionary<int, int>();

            data.LargestPointGap = decimal.MinValue;
            data.SmallestPointGap = decimal.MaxValue;

            foreach (var match in data.MatchesDTO)
            {
                if (match == null) continue;

                var nbOfSets = match.MatchSets?.Count ?? 0;
                if (nbOfSets > 0) data.MatchWithSets++;

                decimal gap = match.Opponent_points_at_match - match.My_points_at_match;

                if (gap > data.LargestPointGap)
                {
                    data.LargestPointGap = gap;
                    data.ResultLargestPointGap = match.Result switch
                    {
                        MatchDTO.MatchResult.V => "Victoire",
                        MatchDTO.MatchResult.D => "Défaite",
                        _ => "Forfait"
                    };
                }

                if (gap < data.SmallestPointGap)
                {
                    data.SmallestPointGap = gap;
                    data.ResultSmallestPointGap = match.Result switch
                    {
                        MatchDTO.MatchResult.V => "Victoire",
                        MatchDTO.MatchResult.D => "Défaite",
                        _ => "Forfait"
                    };
                }

                if (match.Result == MatchDTO.MatchResult.V && gap > data.LargestPointGapWithVictory)
                    data.LargestPointGapWithVictory = gap;

                if (match.Result == MatchDTO.MatchResult.D && (gap < data.SmallestPointGapWithDefeat || data.SmallestPointGapWithDefeat == 0))
                    data.SmallestPointGapWithDefeat = gap;

                int classement = (int)Math.Truncate(match.Opponent_points_at_match / 100);
                classement = classement < 5 ? 5 : classement;

                if (match.Result == MatchDTO.MatchResult.V)
                {
                    if (victoryDistribution.ContainsKey(classement)) victoryDistribution[classement]++;
                    else { victoryDistribution.Add(classement, 1); defeatDistribution.Add(classement, 0); }

                    if (nbOfSets > 0) { data.CounterOfVictoryWithSet++; data.VictoriesNbOfSets[nbOfSets]++; }
                }
                else if (match.Result == MatchDTO.MatchResult.D)
                {
                    if (defeatDistribution.ContainsKey(classement)) defeatDistribution[classement]++;
                    else { defeatDistribution.Add(classement, 1); victoryDistribution.Add(classement, 0); }

                    if (nbOfSets > 0) { data.CounterOfDefeatWithSet++; data.DefeatsNbOfSets[nbOfSets]++; }
                }
                else
                {
                    data.CounterOfWithdraw++;
                }

                data.SortedMatchesDTO[match.Date_of_match.Month].Add(match);

                data.MonthlyPoints += match.Gain;
                DateOnly aMonthLater = match.Date_of_match.AddMonths(1);
                DateOnly nextMonth = new DateOnly(aMonthLater.Year, aMonthLater.Month, 1);

                if (match.Date_of_match.Month == 1 && match.Date_of_match <= season.Phase1_End_date)
                {
                    DateOnly aDate = new DateOnly(match.Date_of_match.Year, match.Date_of_match.Month, 1);
                    data.MonthliesPointsOrdered[aDate] += match.Gain;
                }
                else
                {
                    data.MonthliesPointsOrdered[nextMonth] += match.Gain;
                }

                _matchService.DetermineTypeOfResult(match, ref detailedResults);
            }

            if (data.MatchesDTO.Count > 0)
            {
                data.CounterOfVictory = victoryDistribution.Values.Sum();
                data.CounterOfDefeat = defeatDistribution.Values.Sum();
                data.VictoryPercentage = Math.Round(data.CounterOfVictory / (data.MatchesDTO.Count - data.CounterOfWithdraw) * 100, 2);
            }

            data.DetailedResults = detailedResults;

            foreach (var kv in data.MonthliesPointsOrdered)
            {
                var currentDate = kv.Key;
                var previousMonthDate = currentDate.AddMonths(-1);

                if (data.MonthliesPointsOrdered.ContainsKey(previousMonthDate))
                {
                    data.MonthliesPointsOrdered[currentDate] += data.MonthliesPointsOrdered[previousMonthDate];

                    if (currentDate.Month == 1 || currentDate.Month == 7)
                    {
                        decimal drift = currentDate.Month == 1 ? (season.p1_drift ?? 0) : (season.p2_drift ?? 0);
                        decimal pointsAfterDrift = Math.Max(Math.Round((decimal)data.MonthliesPointsOrdered[currentDate] - drift, 0, MidpointRounding.AwayFromZero), 500);

                        // Si on est en juillet, on n'affiche pas le classement mensuel mais le classement officiel (mensuel - dérive avec application de l'arrondi)
                        if (currentDate.Month == 7)
                        {
                            data.MonthliesPointsOrdered[currentDate] = pointsAfterDrift;
                        }
                        else
                        {
                            data.MonthliesPointsOrdered[currentDate] = Math.Max((decimal)data.MonthliesPointsOrdered[currentDate] - drift, 500);
                        }

                        // Met à jour le classement officiel de mi-saison si la date de fin de phase 1 est passée et que les points officiels de mi-saison ne sont pas renseignés
                        if (DateOnly.FromDateTime(DateTime.Today) > season.Phase1_End_date && data.PointsMiddleOfSeason == null)
                        {
                            data.PlayerSeason.Points_middle = pointsAfterDrift;
                            _context.PlayerSeason.Update(data.PlayerSeason);
                            await _context.SaveChangesAsync();
                            data.PointsMiddleOfSeason = data.PlayerSeason.Points_middle;
                        }
                    }
                }
            }

            if (victoryDistribution.Count != 0)
            {
                int keyMin = victoryDistribution.Keys.Min();
                int keyMax = victoryDistribution.Keys.Max();

                for (int i = keyMin + 1; i < keyMax; i++)
                {
                    if (!victoryDistribution.ContainsKey(i))
                    {
                        victoryDistribution.Add(i, 0);
                        defeatDistribution.Add(i, 0);
                    }
                }
            }

            if (data.MatchesDTO.Count > 0)
            {
                data.VictoryDistributionOrdered = victoryDistribution.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
                data.DefeatDistributionOrdered = defeatDistribution.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);

                var sum = data.VictoryDistributionOrdered.ToDictionary(kv => kv.Key, kv => kv.Value + data.DefeatDistributionOrdered[kv.Key]);
                data.VictoryDefeatMaxGauge = sum.Max(kv => kv.Value);
            }

            foreach (var key in data.SortedMatchesDTO.Keys.ToList())
                data.SortedMatchesDTO[key] = data.SortedMatchesDTO[key].OrderByDescending(m => m.Date_of_match).ToList();

            var maxLimitDay = DateOnly.FromDateTime(DateTime.Today);

            if (maxLimitDay > DateOnly.FromDateTime(new DateTime(season.End_date.Year, 7, 1)))
                maxLimitDay = DateOnly.FromDateTime(new DateTime(season.End_date.Year, 7, 1));

            if (maxLimitDay < DateOnly.FromDateTime(new DateTime(season.Start_date.Year, 9, 1)))
                maxLimitDay = DateOnly.FromDateTime(new DateTime(season.Start_date.Year, 9, 1));

            data.MonthlyPoints = data.MonthliesPointsOrdered[new DateOnly(maxLimitDay.Year, maxLimitDay.Month, 1)];

            data.VirtualPoints = DateTime.Now.Month == 1 && DateOnly.FromDateTime(DateTime.Now) > season.Phase1_End_date
                ? data.MonthlyPoints + (data.SortedMatchesDTO.TryGetValue(maxLimitDay.Month, out var matches)
                    ? matches.Where(m => m.Date_of_match > season.Phase1_End_date).Sum(m => m.Gain) : 0m)
                : data.MonthlyPoints + data.SortedMatchesDTO[maxLimitDay.Month].Sum(m => m.Gain);

            data.MonthLabels = data.MonthliesPointsOrdered
                .Where(kv => kv.Key <= maxLimitDay)
                .Select(kv => CultureInfo.GetCultureInfo("fr-FR").TextInfo.ToTitleCase(kv.Key.ToString("MMMM", CultureInfo.GetCultureInfo("fr-FR"))))
                .ToArray();

            data.MonthValues = data.MonthliesPointsOrdered
                .Where(kv => kv.Key <= maxLimitDay)
                .Select(kv => kv.Value)
                .ToArray();

            if (data.MonthLabels.Length == 0)
            {
                data.MonthLabels = new string[] { CultureInfo.GetCultureInfo("fr-FR").TextInfo.ToTitleCase(season.Start_date.ToString("MMMM", CultureInfo.GetCultureInfo("fr-FR"))) };
                data.MonthValues = new decimal?[] { data.MonthlyPoints };
            }

            data.MonthliesPointsOrderedFiltered = data.MonthliesPointsOrdered
                .Where(kv => kv.Key <= maxLimitDay)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            data.RankingMaxValue = (int)(Math.Ceiling(
                data.MonthValues.Where(v => v.HasValue).Select(v => v.Value).DefaultIfEmpty(500m).Max() / 10m) * 10m);

            return data;
        }

        public async Task CreateSeasonAutomaticallyAsync()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly start, end, p1EndDate;

            if (today.Month >= 9)
            {
                start = new DateOnly(today.Year, 9, 1);
                end = new DateOnly(today.Year + 1, 8, 31);
                p1EndDate = new DateOnly(today.Year + 1, 1, 1);
            }
            else
            {
                start = new DateOnly(today.Year - 1, 9, 1);
                end = new DateOnly(today.Year, 8, 31);
                p1EndDate = new DateOnly(today.Year, 1, 1);
            }

            var newSeason = new Season
            {
                Name = $"Saison {start.Year}-{end.Year}",
                Start_date = start,
                End_date = end,
                Phase1_End_date = p1EndDate
            };

            _context.Season.Add(newSeason);
            await _context.SaveChangesAsync();
        }
    }
}