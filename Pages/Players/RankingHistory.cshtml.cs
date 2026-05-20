using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;
using Microsoft.EntityFrameworkCore;

namespace TableTennisHistoric.Pages.Players
{
    public class RankingHistoryModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;
        public List<PlayerSeason> PlayerSeasons { get; set; } = new();
        public Dictionary<string, decimal?> RankingHistory { get; set; } = new();
        public int RankingMaxValue { get; set; }

        public RankingHistoryModel(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            await LoadPlayerSeasons();
        }

        private async Task LoadPlayerSeasons()
        {
            PlayerSeasons = await _context.PlayerSeason
                .Where(ps => ps.Player.Is_me)
                .Include(ps => ps.Season)
                .OrderBy(ps => ps.Season.Start_date)
                .ToListAsync();

            foreach (var playerSeason in PlayerSeasons)
            {
                RankingHistory.Add("Sept. 20" + playerSeason.Season.Start_date.Year.ToString()[^2..], playerSeason.Points_start ?? 0);

                // Si on est sur la deuxième moitié de saison, les points officiels de mi-saison sont affichés
                if (DateOnly.FromDateTime(DateTime.Now) > playerSeason.Season.Start_date.AddMonths(4))
                {
                    RankingHistory.Add("Jan. 20" + playerSeason.Season.End_date.Year.ToString()[^2..], playerSeason.Points_middle ?? 0);
                }
            }

            RankingMaxValue = (int)RankingHistory.Values
                .OfType<decimal>()
                .DefaultIfEmpty(500m)
                .Max();

            RankingMaxValue = (int)(Math.Ceiling(RankingMaxValue / 10m) * 10m) + 20;
        }
    }
}
