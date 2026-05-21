using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages.Championship
{
    public class ReadModel : PageModel
    {
        private readonly IChampionshipService _championshipService;

        public ReadModel(IChampionshipService championshipService)
        {
            _championshipService = championshipService;
        }

        public List<SelectListItem> ChampionshipSelectList { get; set; } = new();

        [BindProperty]
        public int? SelectedChampionshipId { get; set; }

        [BindProperty]
        public List<MatchDisplayModel> Matches { get; set; } = new();

        public Models.Championship? Championship { get; set; }

        public List<TeamResult> TeamResults { get; set; } = new();

        public class MatchDisplayModel
        {
            public int Id { get; set; }
            public int Matchday { get; set; }
            public int? Home_score { get; set; }
            public int? Away_score { get; set; }
            public string? HomeTeamName { get; set; }
            public string? AwayTeamName { get; set; }
        }

        public class TeamResult
        {
            public string Name { get; set; } = "";
            public int NbOfGamePlayed { get; set; } = 0;
            public int Points { get; set; } = 0;
            public int Victory { get; set; } = 0;
            public int Draw { get; set; } = 0;
            public int Defeat { get; set; } = 0;
            public int? GameWon { get; set; } = 0;
            public int? GameLost { get; set; } = 0;
            public int? GameAverage { get; set; } = 0;
        }

        public async Task OnGetAsync()
        {
            ChampionshipSelectList = await _championshipService.GetChampionshipSelectListAsync();

            if (TempData.ContainsKey("SelectedChampionshipId"))
            {
                SelectedChampionshipId = (int?)TempData["SelectedChampionshipId"];
                TempData.Keep("SelectedChampionshipId");

                if (SelectedChampionshipId.HasValue)
                    await LoadChampionshipDataAsync(SelectedChampionshipId.Value);
            }
        }

        public async Task<IActionResult> OnPostSelectChampionshipAsync()
        {
            ModelState.Clear();
            ChampionshipSelectList = await _championshipService.GetChampionshipSelectListAsync();

            if (!SelectedChampionshipId.HasValue)
            {
                ModelState.AddModelError(nameof(SelectedChampionshipId), "Veuillez sélectionner un championnat.");
                return Page();
            }

            await LoadChampionshipDataAsync(SelectedChampionshipId.Value);
            return Page();
        }

        public async Task<IActionResult> OnPostSaveScoresAsync()
        {
            ModelState.Clear();

            if (Matches == null)
            {
                ModelState.AddModelError(string.Empty, "Aucun match à enregistrer.");
                ChampionshipSelectList = await _championshipService.GetChampionshipSelectListAsync();
                return Page();
            }

            await _championshipService.SaveScoresAsync(Matches.Select(m => (m.Id, m.Home_score, m.Away_score)).ToList());

            TempData["Success"] = "Scores enregistrés !";
            TempData["SelectedChampionshipId"] = SelectedChampionshipId;
            return RedirectToPage();
        }

        private async Task LoadChampionshipDataAsync(int id)
        {
            var data = await _championshipService.GetChampionshipDataAsync(id);
            Matches = data.Matches.Select(m => new MatchDisplayModel
            {
                Id = m.Id,
                Matchday = m.Matchday,
                Home_score = m.Home_score,
                Away_score = m.Away_score,
                HomeTeamName = m.HomeTeamName,
                AwayTeamName = m.AwayTeamName
            }).ToList();
            Championship = data.Championship;
            TeamResults = data.TeamResults.Select(t => new TeamResult
            {
                Name = t.Name,
                NbOfGamePlayed = t.NbOfGamePlayed,
                Points = t.Points,
                Victory = t.Victory,
                Draw = t.Draw,
                Defeat = t.Defeat,
                GameWon = t.GameWon,
                GameLost = t.GameLost,
                GameAverage = t.GameAverage
            }).ToList();
        }
    }
}