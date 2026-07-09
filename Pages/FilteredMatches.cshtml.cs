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
        public SelectList Competitions { get; set; } = null!;
        public SelectList Clubs { get; set; } = null!;

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public MatchFilterDTO Filter { get; set; } = new();
        public bool DisplayWinDefeatPercentage { get; set; } = true;
        public decimal? VictoryPercentage { get; set; } = 0;
        public string? Performance { get; set; }


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

            DisplayWinDefeatPercentage = CheckForDisplayWinDefeatPercentage();
            if (DisplayWinDefeatPercentage)
            {
                VictoryPercentage = CalculateVictoryPercentage();
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

            if (!Filter.Perf.Equals(""))
            {
                return false;
            }

            // Détermine les filtres qui vont déclencher l'affichage du pourcentage de victoires/défaites
            if (Filter.SeasonId.HasValue
                || Filter.CompetitionId.HasValue
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
                || Filter.ClubId.HasValue
                || Filter.DateFrom.HasValue
                || Filter.DateTo.HasValue
                || Filter.Result.HasValue
                || Filter.NbOfSets.HasValue
                || Filter.OpponentPointsMin.HasValue
                || Filter.OpponentPointsMax.HasValue
                || !Filter.Perf.Equals("");
        }

        private async Task LoadSelectListsAsync()
        {
            var seasons = await _seasonService.GetAllSeasonsAsync();
            Seasons = new SelectList(seasons?.OrderByDescending(s => s.Start_date), "Id", "Name");

            var competitions = await _competitionService.GetAllCompetitionAsync();
            Competitions = new SelectList(competitions?.OrderBy(c => c.Name), "Id", "Name");

            Clubs = await _clubService.GetClubsSelectListAsync();
        }
    }
}
