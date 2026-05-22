using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Players
{
    public class RankingHistoryModel : PageModel
    {
        private readonly IPlayerService _playerService;

        public RankingHistoryModel(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        public Dictionary<string, decimal?> RankingHistory { get; set; } = new();
        public int RankingMaxValue { get; set; }

        public async Task OnGetAsync()
        {
            var player = await _playerService.GetMeAsync();
            var data = await _playerService.GetRankingHistoryByPlayerIdAsync(player.Id);
            RankingHistory = data.RankingHistory;
            RankingMaxValue = data.RankingMaxValue;
        }
    }
}