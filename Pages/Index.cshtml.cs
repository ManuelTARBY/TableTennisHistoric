using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class IndexModel : PageModel
    {
        public Player? PlayerMe { get; set; } = new();
        public List<MatchDTO> MatchesDTO { get; set; } = new();
        public SeasonDTO? SeasonDTO { get; set; }
        public bool ShowSeasonModal { get; set; } = false;
        public Season NewSeason { get; set; } = new();
        public PlayerSeason PlayerSeason { get; set; } = new();
        public decimal CounterOfVictory { get; set; } = 0;
        public decimal CounterOfVictoryWithSet { get; set; } = 0;
        public decimal CounterOfDefeat { get; set; } = 0;
        public decimal CounterOfDefeatWithSet { get; set; } = 0;
        public decimal CounterOfWithdraw { get; set; } = 0;
        public decimal? PointsBeginningSeason { get; set; }
        public decimal? PointsMiddleOfSeason { get; set; }
        public decimal? VirtualPoints { get; set; } = 0;
        public decimal? MonthlyPoints { get; set; }
        public decimal? VictoryPercentage { get; set; } = 0;
        public decimal LargestPointGapWithVictory { get; set; }
        public decimal LargestPointGap { get; set; }
        public string ResultLargestPointGap { get; set; }
        public decimal SmallestPointGap { get; set; }
        public decimal SmallestPointGapWithDefeat { get; set; }
        public string ResultSmallestPointGap { get; set; }
        public decimal MatchWithSets { get; set; } = 0;
        public List<SelectListItem> SeasonsSelectList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int SelectedSeasonId { get; set; }
        public Dictionary<int?, int> VictoriesNbOfSets = new()
        {
            {3, 0},
            {4, 0},
            {5, 0},
        };
        public Dictionary<int?, int> DefeatsNbOfSets = new()
        {
            {3, 0},
            {4, 0},
            {5, 0},
        };
        public Dictionary<string, int> DetailedResults { get; set; } = new();
        private IMatchService _matchService { get; set; }
        private IPlayerService _playerService { get; set; }
        private ISeasonService _seasonService { get; set; }
        private TableTennisHistoricDbContext _context { get; set; }

        public string[] MonthLabels { get; set; }

        public decimal?[] MonthValues { get; set; }
        public int RankingMaxValue { get; set; }

        public Dictionary<int, List<MatchDTO>> SortedMatchesDTO = new()
            {
                { 9, new List<MatchDTO>() },
                { 10, new List<MatchDTO>() },
                { 11, new List<MatchDTO>() },
                { 12, new List<MatchDTO>() },
                { 1, new List<MatchDTO>() },
                { 2, new List<MatchDTO>() },
                { 3, new List<MatchDTO>() },
                { 4, new List<MatchDTO>() },
                { 5, new List<MatchDTO>() },
                { 6, new List<MatchDTO>() },
                { 7, new List<MatchDTO>() },
                { 8, new List<MatchDTO>() }
            };

        public Dictionary<DateOnly, decimal?> MonthliesPointsOrdered { get; set; } = new();
        public Dictionary<DateOnly, decimal?> MonthliesPointsOrderedFiltered { get; set; } = new();

        public Dictionary<int, int> OpponentDistribution = new Dictionary<int, int>();
        public Dictionary<int, int> VictoryDistribution = new Dictionary<int, int>();
        public Dictionary<int, int> VictoryDistributionOrdered = new Dictionary<int, int>();
        public Dictionary<int, int> DefeatDistribution = new Dictionary<int, int>();
        public Dictionary<int, int> DefeatDistributionOrdered = new Dictionary<int, int>();
        public int VictoryDefeatMaxGauge { get; set; } = 0;

        public IndexModel(TableTennisHistoricDbContext context, IMatchService matchService, IPlayerService playerService, ISeasonService seasonService)
        {
            _matchService = matchService;
            _playerService = playerService;
            _seasonService = seasonService;
            _context = context;
        }

        public async Task OnGetAsync()
        {

            var seasons = await _seasonService.GetAllSeasonsAsync();

            SeasonsSelectList = seasons
                .OrderByDescending(s => s.Start_date)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToList();

            // Sélection de la saison à afficher : priorité à la saison choisie dans le sélecteur, sinon à la saison mémorisée en session, sinon à la saison courante, sinon affichage du modal de création de saison
            if (SelectedSeasonId == 0)
            {
                // Vérifie si une saison est mémorisée en session
                var sessionValue = HttpContext.Session.GetString("SelectedSeasonId");

                if (sessionValue != null && int.TryParse(sessionValue, out int sessionSeasonId)
                    && seasons.Any(s => s.Id == sessionSeasonId))
                {
                    // La saison de la session existe bien en base → on l'utilise
                    SelectedSeasonId = sessionSeasonId;
                    var sessionSeason = seasons.First(s => s.Id == sessionSeasonId);
                    SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(sessionSeason);
                }
                else
                {
                    // Pas de session valide → saison courante par défaut
                    var currentSeason = await _seasonService.GetCurrentSeasonAsync();
                    if (currentSeason == null)
                    {
                        ShowSeasonModal = true;
                        return;
                    }
                    SelectedSeasonId = currentSeason.Id;
                    SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(currentSeason);
                }
            }
            else
            {
                // Saison explicitement choisie → on met à jour la session
                var selectedSeason = seasons.First(s => s.Id == SelectedSeasonId);
                SeasonDTO = _seasonService.ConvertSeasonToSeasonDTO(selectedSeason);

                HttpContext.Session.SetString("SelectedSeasonId", SelectedSeasonId.ToString());
            }

            Season season = await _seasonService.GetSeasonByIdAsync(SelectedSeasonId);

            // Récupère les matchs de la saison sélectionnée
            MatchesDTO = await _matchService.GetAllMatchesDTOAsync();

            MatchesDTO = MatchesDTO
                .Where(m => m.SeasonId == SelectedSeasonId)
                .ToList();

            // Récupère les points au début et au milieu de la saison
            PlayerMe = await _playerService.GetMeAsync();
            if (PlayerMe != null)
            {
                PlayerSeason = await _playerService.GetPlayerSeasonByPlayerAndSeasonAsync(PlayerMe, season);
                PointsBeginningSeason = PlayerSeason.Points_start;
            }
            MonthlyPoints = PointsBeginningSeason ?? 0;

            for (int i = 0; i < 12; i++)
            {
                if (i == 0)
                {
                    MonthliesPointsOrdered.Add(season.Start_date.AddMonths(i), PointsBeginningSeason);
                }
                else
                {
                    MonthliesPointsOrdered.Add(season.Start_date.AddMonths(i), 0);
                }
            }

            Dictionary<string, int> FakeDetailedResults = new()
            {
                { "Perfs", 0 },
                { "Victoires normales", 0 },
                { "Défaites normales", 0 },
                { "Contre-perfs", 0 },
                { "Forfaits", 0 }
            };

            LargestPointGap = decimal.MinValue;
            SmallestPointGap = decimal.MaxValue;
            LargestPointGapWithVictory = 0;
            SmallestPointGapWithDefeat = 0;

            // Parcourt les matches pour construire le DTO de chacun
            foreach (var match in MatchesDTO)
            {

                if (match == null) { continue; }

                // Calcule le nombre de match avec sets renseignés
                var nbOfSets = match.MatchSets?.Count ?? 0;
                if (nbOfSets > 0) { MatchWithSets++; }

                // Calcule le plus grand écart de points et le plus petit écart de points
                decimal gap = match.Opponent_points_at_match - match.My_points_at_match;

                if (gap > LargestPointGap)
                {
                    LargestPointGap = gap;
                    ResultLargestPointGap = match.Result switch
                    {
                        MatchDTO.MatchResult.V => "Victoire",
                        MatchDTO.MatchResult.D => "Défaite",
                        _ => "Forfait"
                    };
                }
                if (gap < SmallestPointGap)
                {
                    SmallestPointGap = gap;
                    ResultSmallestPointGap = match.Result switch
                    {
                        MatchDTO.MatchResult.V => "Victoire",
                        MatchDTO.MatchResult.D => "Défaite",
                        _ => "Forfait"
                    };
                }

                // Meilleur et pire écart spécifique
                if (match.Result == MatchDTO.MatchResult.V && gap > LargestPointGapWithVictory)
                    LargestPointGapWithVictory = gap;
                if (match.Result == MatchDTO.MatchResult.D && (gap < SmallestPointGapWithDefeat || SmallestPointGapWithDefeat == 0))
                    SmallestPointGapWithDefeat = gap;

                // Calcule du ratio de victoire et la distribution des victoires/défaites par classement de l'adversaire
                int classement = (int)Math.Truncate(match.Opponent_points_at_match / 100);
                classement = classement < 5 ? 5 : classement;

                if (match.Result == (MatchDTO.MatchResult)TableTennisMatch.MatchResult.V)
                {

                    if (VictoryDistribution.ContainsKey(classement))
                    {
                        VictoryDistribution[classement]++;
                    }
                    else
                    {
                        VictoryDistribution.Add(classement, 1);
                        DefeatDistribution.Add(classement, 0);
                    }

                    if (nbOfSets > 0)
                    {
                        CounterOfVictoryWithSet++;
                        VictoriesNbOfSets[nbOfSets]++;
                    }
                }
                else if (match.Result == (MatchDTO.MatchResult)TableTennisMatch.MatchResult.D)
                {

                    if (DefeatDistribution.ContainsKey(classement))
                    {
                        DefeatDistribution[classement]++;
                    }
                    else
                    {
                        DefeatDistribution.Add(classement, 1);
                        VictoryDistribution.Add(classement, 0);
                    }

                    if (nbOfSets > 0)
                    {
                        CounterOfDefeatWithSet++;
                        DefeatsNbOfSets[nbOfSets]++;
                    }
                }
                else
                {
                    CounterOfWithdraw++;
                }

                SortedMatchesDTO[match.Date_of_match.Month].Add(match);

                // Calcule les points mensuels pour chaque mois
                MonthlyPoints += match.Gain;
                DateOnly aMonthLater = match.Date_of_match.AddMonths(1);
                DateOnly nextMonth = new DateOnly(aMonthLater.Year, aMonthLater.Month, 1);
                if (match.Date_of_match.Month == 1 && match.Date_of_match <= season.Phase1_End_date)
                {
                    DateOnly aDate = new DateOnly(match.Date_of_match.Year, match.Date_of_match.Month, 1);
                    MonthliesPointsOrdered[aDate] += match.Gain;
                }
                else
                {
                    MonthliesPointsOrdered[nextMonth] += match.Gain;
                }

                // Répartit les matchs en perf/contre-perf/normale
                _matchService.DetermineTypeOfResult(match, ref FakeDetailedResults);

            }

            if (MatchesDTO.Count > 0)
            {
                CounterOfVictory = VictoryDistribution.Values.Sum();
                CounterOfDefeat = DefeatDistribution.Values.Sum();
                VictoryPercentage = Math.Round(CounterOfVictory / (MatchesDTO.Count - CounterOfWithdraw) * 100, 2);
            }

            DetailedResults = FakeDetailedResults;

            // Calcul des points par mois
            foreach (var kv in MonthliesPointsOrdered)
            {
                var currentDate = kv.Key;
                var previousMonthDate = currentDate.AddMonths(-1);

                if (MonthliesPointsOrdered.ContainsKey(previousMonthDate))
                {
                    MonthliesPointsOrdered[currentDate] += MonthliesPointsOrdered[previousMonthDate];
                    // S'il y a des points mensuels à calculer pour janvier ou juillet, on applique la dérive de points
                    if (currentDate.Month == 1 || currentDate.Month == 7)
                    {
                        decimal Drift = currentDate.Month == 1 ? (season.p1_drift ?? 0) : (season.p2_drift ?? 0);
                        decimal PointsAfterDrift = Math.Max(Math.Round((decimal)MonthliesPointsOrdered[currentDate] - Drift, 0, MidpointRounding.AwayFromZero), 500);
                        MonthliesPointsOrdered[currentDate] = Math.Max((decimal)MonthliesPointsOrdered[currentDate] - Drift, 500);

                        if (DateOnly.FromDateTime(DateTime.Today) > season.Phase1_End_date && PointsMiddleOfSeason == null)
                        {
                            PlayerSeason.Points_middle = PointsAfterDrift;

                            // Indique au contexte que l'objet a été modifié
                            _context.PlayerSeason.Update(PlayerSeason);

                            // Enregistre les changements dans la base
                            await _context.SaveChangesAsync();

                            // Met à jour la variable PointsMiddleOfSeason
                            PointsMiddleOfSeason = PlayerSeason.Points_middle;
                        }
                    }
                }
            }


            // Crée les classements intermédiaires non rencontrés 
            if (VictoryDistribution.Count != 0)
            {
                int keyMin = VictoryDistribution.Keys.Min();
                int keyMax = VictoryDistribution.Keys.Max();

                for (int i = keyMin + 1; i < keyMax; i++)
                {
                    if (VictoryDistribution.ContainsKey(i) == false)
                    {
                        VictoryDistribution.Add(i, 0);
                        DefeatDistribution.Add(i, 0);
                    }
                }
            }

            // Tri des matchs par ordre inversement chronologique
            if (MatchesDTO.Count > 0)
            {
                foreach (var key in SortedMatchesDTO.Keys.ToList())
                {
                    SortedMatchesDTO[key] = SortedMatchesDTO[key]
                        .OrderByDescending(m => m.Date_of_match)
                        .ToList();
                }

                // Tri des distributions des victoires et défaites (de classement 5 au classement max)
                VictoryDistributionOrdered = VictoryDistribution
                    .OrderBy(kv => kv.Key)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                DefeatDistributionOrdered = DefeatDistribution
                    .OrderBy(kv => kv.Key)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                // Calcule le classement le plus affronté pour déterminer l'échelle max du graphique
                Dictionary<int, int> Sum = new Dictionary<int, int>();
                foreach (var (key, value) in VictoryDistributionOrdered)
                {
                    Sum.Add(key, value);
                    Sum[key] += DefeatDistributionOrdered[key];
                }
                VictoryDefeatMaxGauge = Sum.Max(kv => kv.Value);

                // Mise à jour des points virtuels
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (DateOnly.FromDateTime(DateTime.Today) > season.End_date)
                {
                    today = new DateOnly(season.End_date.Year, 7, 1);
                }

                MonthlyPoints = MonthliesPointsOrdered[new DateOnly(today.Year, today.Month, 1)];
                if (DateTime.Now.Month == 1 && DateOnly.FromDateTime(DateTime.Now) > season.Phase1_End_date)
                {
                    VirtualPoints = MonthlyPoints + (SortedMatchesDTO.TryGetValue(today.Month, out var matches)
                        ? matches.Where(m => m.Date_of_match > season.Phase1_End_date).Sum(m => m.Gain) : 0m);
                }
                else
                {
                    VirtualPoints = MonthlyPoints + SortedMatchesDTO[today.Month].Sum(m => m.Gain);
                }

                // Blocage des données pour ne pas dépasser le mois précédant le mois en cours dans le graphique d'évolution des points
                MonthLabels = MonthliesPointsOrdered
                    .Where(kv => kv.Key <= today)
                    .Select(kv => CultureInfo.GetCultureInfo("fr-FR")
                        .TextInfo.ToTitleCase(kv.Key.ToString("MMMM", CultureInfo.GetCultureInfo("fr-FR"))))
                    .ToArray();

                MonthValues = MonthliesPointsOrdered
                    .Where(kv => kv.Key <= today)
                    .Select(kv => kv.Value)
                    .ToArray();

                MonthliesPointsOrderedFiltered = MonthliesPointsOrdered
                    .Where(kv => kv.Key <= today)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                RankingMaxValue = (int)Math.Round(MonthValues.Where(v => v.HasValue)
                   .Select(v => v.Value)
                   .DefaultIfEmpty(500m)
                   .Max(), 0);

                RankingMaxValue = (int)(Math.Ceiling(RankingMaxValue / 10m) * 10m);
            }
        }

        /**
         * Crée une saison en cours 
         */
        public async Task<IActionResult> OnPostCreateSeasonAutomaticallyAsync()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            DateOnly start, end, p1_End_Date;

            if (today.Month >= 9)
            {
                start = new DateOnly(today.Year, 9, 1);
                end = new DateOnly(today.Year + 1, 8, 31);
                p1_End_Date = new DateOnly(today.Year + 1, 1, 1);
            }
            else
            {
                start = new DateOnly(today.Year - 1, 9, 1);
                end = new DateOnly(today.Year, 8, 31);
                p1_End_Date = new DateOnly(today.Year, 1, 1);
            }

            NewSeason.Name = $"Saison {start.Year}-{end.Year}";
            NewSeason.Start_date = start;
            NewSeason.End_date = end;
            NewSeason.Phase1_End_date = p1_End_Date;

            _context.Season.Add(NewSeason);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        // Suppression d'un match
        public async Task<IActionResult> OnPostDeleteAsync(int matchId)
        {
            var match = await _context.TableTennisMatch.FindAsync(matchId);
            if (match != null)
            {
                _context.TableTennisMatch.Remove(match);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
