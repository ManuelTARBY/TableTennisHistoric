using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class FilteredMatches: PageModel
    {
        private readonly IMatchService _matchService;
        private readonly ISeasonService _seasonService;
        private readonly ICompetitionService _competitionService;
        private readonly IClubService _clubService;

        public FilteredMatches(IMatchService matchService, ISeasonService seasonService,
            ICompetitionService competitionService, IClubService clubService)
        {
            _matchService = matchService;
            _seasonService = seasonService;
            _competitionService = competitionService;
            _clubService = clubService;
        }

        public List<MatchDTO> Matches { get; set; } = new();
        public SelectList Seasons { get; set; } = null!;
        public List<SelectListItem> Competitions { get; set; } = null!;
        public List<SelectListItem> CompetitionSupplements { get; set; } = null!;
        public SelectList Clubs { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public MatchFilterDTO Filter { get; set; } = new();
        public bool DisplayWinDefeatPercentage { get; set; } = true;
        public decimal? VictoryPercentage { get; set; } = 0;
        public decimal TotalPointsWon { get; set; } = 0;
        public decimal TotalPointsLost { get; set; } = 0;
        public decimal OpponentPointsMin { get; set; } = 0;
        public decimal OpponentPointsMax { get; set; } = 0;
        public decimal OpponentPointsAverage { get; set; } = 0;
        public decimal OpponentPointsMedian { get; set; } = 0;


        public async Task OnGetAsync()
        {
            await LoadSelectListsAsync();

            if (HasAnyFilter())
            {
                Matches = await _matchService.GetFilteredMatchesAsync(Filter);
            }
            else
            {
                Matches = await _matchService.GetAllMatchesDTOAsync();
                Matches = Matches.OrderByDescending(m => m.Date_of_match).ThenBy(m => m.Id).ToList();
            }

            TotalPointsWon = Matches.Where(m => m.Result == MatchDTO.MatchResult.V).Sum(m => m.Gain);
            TotalPointsLost = Matches.Where(m => m.Result == MatchDTO.MatchResult.D).Sum(m => m.Gain);

            DisplayWinDefeatPercentage = CheckForDisplayWinDefeatPercentage();
            if (DisplayWinDefeatPercentage)
            {
                VictoryPercentage = CalculateVictoryPercentage();
            }

            // Calcul des points moyens, min, max et médiane des adversaires
            if (Matches.Count > 0)
            {
                var points = Matches
                    .Where(m => m.Result != MatchDTO.MatchResult.F)
                    .Select(m => m.Opponent_points_at_match)
                    .OrderBy(p => p)
                    .ToList();

                if (points.Count > 0)
                {
                    OpponentPointsMin = points.First();
                    OpponentPointsMax = points.Last();
                    OpponentPointsAverage = Math.Round(points.Average(), 3);

                    // Médiane
                    int mid = points.Count / 2;
                    OpponentPointsMedian = points.Count % 2 == 0
                        ? Math.Round((points[mid - 1] + points[mid]) / 2, 3)
                        : Math.Round(points[mid], 3);
                }
            }
        }

        public decimal CalculateVictoryPercentage()
        {
            int victories = Matches.Count(m => m.Result == MatchDTO.MatchResult.V);
            decimal percentage = (decimal)victories / Matches.Count * 100;
            return Math.Round(percentage, 2);
        }

        public bool CheckForDisplayWinDefeatPercentage()
        {
            if (Matches.Count == 0 || Filter.Result.HasValue)
            {
                return false;
            }

            if (!Filter.Perf.Equals("None"))
            {
                return false;
            }

            // Détermine les filtres qui vont déclencher l'affichage du pourcentage de victoires/défaites
            if (Filter.SeasonId.HasValue
                || Filter.CompetitionId.HasValue
                || Filter.CompetitionSupplementId.HasValue
                || Filter.ClubId.HasValue
                || (Filter.DateFrom.HasValue && Filter.DateTo.HasValue)
                || Filter.NbOfSets.HasValue
                || Filter.OpponentPointsMin.HasValue
                || Filter.OpponentPointsMax.HasValue)
            {
                return true;
            }

            // Si pas de filtres spécifiques (cas du lancement de la page), on affiche le pourcentage
            if (HasAnyFilter() == false)
            {
                return true;
            }

            return false;
        }

        private bool HasAnyFilter()
        {
            return Filter.SeasonId.HasValue
                || Filter.CompetitionId.HasValue
                || Filter.CompetitionSupplementId.HasValue
                || Filter.ClubId.HasValue
                || Filter.DateFrom.HasValue
                || Filter.DateTo.HasValue
                || Filter.Result.HasValue
                || Filter.NbOfSets.HasValue
                || Filter.OpponentPointsMin.HasValue
                || Filter.OpponentPointsMax.HasValue
                || !Filter.Perf.Equals("None");
        }

        private async Task LoadSelectListsAsync()
        {
            var seasons = await _seasonService.GetAllSeasonsAsync();
            Seasons = new SelectList(seasons?.OrderByDescending(s => s.Start_date), "Id", "Name");

            var competitions = await _competitionService.GetAllCompetitionAsync();

            var groupTournois = new SelectListGroup { Name = "Tournois" };
            var groupCriterium = new SelectListGroup { Name = "Critérium" };
            var groupAutres = new SelectListGroup { Name = "Autres" };

            Competitions = competitions.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Group = c.Name.Contains("Tournoi", StringComparison.OrdinalIgnoreCase) ? groupTournois :
                (c.Name.Contains("Critérium", StringComparison.OrdinalIgnoreCase)) ? groupCriterium : groupAutres
            }).ToList();

            // Ajouter l'option "Groupe Tournoi" globale à la liste
            Competitions.Insert(0, new SelectListItem
            {
                Value = "-1",
                Text = "Tous les tournois",
                Group = groupTournois
            });
            // Ajouter l'option "Groupe Tournoi" globale à la liste
            Competitions.Insert(1, new SelectListItem
            {
                Value = "-2",
                Text = "Tous les tours de critérium",
                Group = groupCriterium
            });

            Clubs = await _clubService.GetClubsSelectListAsync();

            var competitionSupplements = await _competitionService.GetAllCompetitionSupplementAsync();
            CompetitionSupplements = competitionSupplements.Select(cs => new SelectListItem
            {
                Value = cs.Id.ToString(),
                Text = cs.Name
            })
            .OrderBy(cc => cc.Text)
            .ToList();
        }
    }
}
