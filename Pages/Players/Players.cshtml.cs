using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Pages;
public class PlayersModel : PageModel
{
    private readonly TableTennisHistoricDbContext _context;

    public PlayersModel(TableTennisHistoricDbContext context)
    {
        _context = context;
    }

    public Season? ActiveSeason { get; set; } = new();
    public List<PlayerWithClub> Players { get; set; } = new();

    public async Task OnGetAsync()
    {
        await GetSeasonAsync();

        Players = await _context.Player
             .Include(p => p.PlayerSeasons)
                 .ThenInclude(pc => pc.Club)
             .Where(p => p.PlayerSeasons.Any(pc => pc.SeasonId == ActiveSeason.Id))
             .Select(p => new PlayerWithClub
             {
                 Player = p,
                 Club = p.PlayerSeasons
                     .Where(pc => pc.SeasonId == ActiveSeason.Id)
                     .Select(pc => pc.Club)
                     .FirstOrDefault()
             })
             .ToListAsync();
    }

    public async Task GetSeasonAsync()
    {
        DateOnly BeginningOfSeason;
        if (DateTime.Now.Month >= 9)
        {
            BeginningOfSeason = new DateOnly(DateTime.Now.Year, 09, 01);
        }
        else
        {
            BeginningOfSeason = new DateOnly(DateTime.Now.Year - 1, 09, 01);
        }
        ActiveSeason = await _context.Season.FirstOrDefaultAsync(s => s.Start_date == BeginningOfSeason);
    }

    public class PlayerWithClub
    {
        public Player Player { get; set; } = null!;
        public Club? Club { get; set; }
    }
}
