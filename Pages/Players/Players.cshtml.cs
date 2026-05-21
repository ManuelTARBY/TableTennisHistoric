using Microsoft.AspNetCore.Mvc.RazorPages;
using TableTennisHistoric.DTO;
using TableTennisHistoric.Models;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages;

public class PlayersModel : PageModel
{
    private readonly IPlayerService _playerService;
    private readonly ISeasonService _seasonService;

    public PlayersModel(IPlayerService playerService, ISeasonService seasonService)
    {
        _playerService = playerService;
        _seasonService = seasonService;
    }

    public Season? ActiveSeason { get; set; }
    public List<PlayerWithClubDTO> Players { get; set; } = new();

    public class PlayerWithClub
    {
        public Player Player { get; set; } = null!;
        public Club? Club { get; set; }
    }

    public async Task OnGetAsync()
    {
        ActiveSeason = await _seasonService.GetCurrentSeasonAsync();

        if (ActiveSeason == null) return;

        Players = await _playerService.GetPlayersWithClubBySeasonAsync(ActiveSeason);
    }
}