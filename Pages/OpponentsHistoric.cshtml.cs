using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class OpponentsHistoric : PageModel
    {
        private readonly IMatchService _matchService;
        private readonly IPlayerService _playerService;

        public OpponentsHistoric(IMatchService matchService, IPlayerService playerService)
        {
            _matchService = matchService;
            _playerService = playerService;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedOpponentId { get; set; }

        public List<MatchDTO> Matches { get; set; } = new();
        public SelectList OpponentsSelectList { get; set; } = new SelectList(Enumerable.Empty<object>(), "Id", "FullName");
        public decimal? VictoryPercentage { get; set; }
        public decimal CounterOfVictory { get; set; } = 0;
        public decimal CounterOfDefeat { get; set; } = 0;
        public decimal CounterOfWithdraw { get; set; } = 0;

        public Dictionary<string, decimal?> RankingHistory { get; set; } = new();
        public int RankingMaxValue { get; set; }

        public async Task OnGetAsync()
        {
            OpponentsSelectList = await _playerService.GetOpponentsSelectListAsync();

            if (SelectedOpponentId.HasValue)
            {
                Matches = await _matchService.GetMatchesByOpponentAsync(SelectedOpponentId.Value);

                if (Matches.Count > 0)
                {
                    CounterOfVictory = Matches.Count(m => m.Result == MatchDTO.MatchResult.V);
                    CounterOfDefeat = Matches.Count(m => m.Result == MatchDTO.MatchResult.D);
                    CounterOfWithdraw = Matches.Count(m => m.Result == MatchDTO.MatchResult.F);
                    VictoryPercentage = Math.Round(CounterOfVictory / (Matches.Count - CounterOfWithdraw) * 100m, 2);
                }

                // On récupère l'historique de classement
                var rankingHistory = await _playerService.GetRankingHistoryByPlayerIdAsync(SelectedOpponentId);
                RankingHistory = rankingHistory.RankingHistory;
                RankingMaxValue = rankingHistory.RankingMaxValue;
            }

        }

        public IActionResult OnPostSelectOpponentAsync()
        {
            if (!SelectedOpponentId.HasValue)
                return RedirectToPage();

            return RedirectToPage(new { SelectedOpponentId });
        }
    }
}