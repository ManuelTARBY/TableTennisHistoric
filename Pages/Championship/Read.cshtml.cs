using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;

namespace TableTennisHistoric.Pages.Championship
{
    public class ReadModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;

        public ReadModel(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public List<SelectListItem> ChampionshipSelectList { get; set; } = new();

        // Bound selected championship id from the form
        [BindProperty]
        public int? SelectedChampionshipId { get; set; }

        [BindProperty]
        public List<MatchDisplayModel> Matches { get; set; } = new();

        public Models.Championship? Championship { get; set; }

        public List<TeamResult> TeamResults { get; set; } = new();

        // View models
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

        // GET - populate the championships dropdown
        public async Task OnGetAsync()
        {
            await LoadChampionshipSelectListAsync();

            if (TempData.ContainsKey("SelectedChampionshipId"))
            {
                SelectedChampionshipId = (int?)TempData["SelectedChampionshipId"];
                TempData.Keep("SelectedChampionshipId"); // si besoin d'un second accès

                if (SelectedChampionshipId.HasValue)
                    await LoadChampionshipDataAsync(SelectedChampionshipId.Value);
            }
        }

        // POST - when user clicks "Afficher" to show championship
        public async Task<IActionResult> OnPostSelectChampionshipAsync()
        {
            // Clear validation from other forms (safety)
            ModelState.Clear();

            await LoadChampionshipSelectListAsync();

            if (!SelectedChampionshipId.HasValue)
            {
                ModelState.AddModelError(nameof(SelectedChampionshipId), "Veuillez sélectionner un championnat.");
                return Page();
            }

            await LoadChampionshipDataAsync(SelectedChampionshipId.Value);

            return Page();
        }

        // POST - save scores edited in the page
        public async Task<IActionResult> OnPostSaveScoresAsync()
        {
            ModelState.Clear();

            if (Matches == null)
            {
                ModelState.AddModelError(string.Empty, "Aucun match à enregistrer.");
                await LoadChampionshipSelectListAsync();
                return Page();
            }

            foreach (var m in Matches)
            {
                var dbMatch = await _context.ChampionshipMatch.FindAsync(m.Id);
                if (dbMatch != null)
                {
                    dbMatch.Home_score = m.Home_score;
                    dbMatch.Away_score = m.Away_score;
                    _context.ChampionshipMatch.Update(dbMatch);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Scores enregistrés !";
            TempData["SelectedChampionshipId"] = SelectedChampionshipId;
            return RedirectToPage();
        }


        // Remplit la liste déroulante des championnats
        private async Task LoadChampionshipSelectListAsync()
        {
            var list = await _context.Championship
                .Include(cs => cs.Season)
                .OrderByDescending(cs => cs.Beginning)
                .ThenByDescending(cs => cs.Season.Start_date)
                .ThenByDescending(cs => cs.Phase)
                .ThenBy(cs => cs.Rank)
                .ThenBy(cs => cs.Level)
                .ThenBy(cs => cs.ChampionshipGroup)
                .ToListAsync();

            ChampionshipSelectList = list.Select(cs => new SelectListItem
            {
                Value = cs.Id.ToString(),
                Text = $"{cs.Season?.Name ?? "?"} | Phase {cs.Phase} | {Models.Championship.RankLabels.GetValueOrDefault(cs.Rank, cs.Rank.ToString())} {cs.Level} | Poule {cs.ChampionshipGroup}"
            }).ToList();
        }

        // Charge matches + championship + calcule classement
        private async Task LoadChampionshipDataAsync(int id)
        {
            // Récupère les matches en projetant Home/Away team names via sous-requête join ChampionshipTeam -> Team
            Matches = await _context.ChampionshipMatch
                .Where(m => m.ChampionshipId == id)
                .Select(m => new MatchDisplayModel
                {
                    Id = m.Id,
                    Matchday = m.Matchday,
                    Home_score = m.Home_score,
                    Away_score = m.Away_score,
                    // sous-requête pour récupérer le nom de l'équipe correspondant à Home_teamId (qui est l'id de ChampionshipTeam)
                    HomeTeamName = _context.ChampionshipTeam
                                    .Where(ct => ct.Id == m.Home_teamId)
                                    .Select(ct => ct.Team.Name)
                                    .FirstOrDefault(),
                    AwayTeamName = _context.ChampionshipTeam
                                    .Where(ct => ct.Id == m.Away_teamId)
                                    .Select(ct => ct.Team.Name)
                                    .FirstOrDefault()
                })
                .OrderBy(m => m.Matchday)
                .ThenBy(m => m.Id)
                .AsNoTracking()
                .ToListAsync();

            // charge l'entité championnat (pour affichage titre)
            Championship = await _context.Championship
                .Include(cs => cs.Season)
                .FirstOrDefaultAsync(cs => cs.Id == id);

            // calcule le classement
            ComputeRanking();
        }

        // Calcul du classement à partir de Matches
        private void ComputeRanking()
        {
            TeamResults = new List<TeamResult>();

            if (Matches == null || !Matches.Any()) return;

            foreach (var match in Matches)
            {
                // s'assure que les noms existent
                var homeName = match.HomeTeamName ?? "N/C";
                var awayName = match.AwayTeamName ?? "N/C";

                var home = TeamResults.FirstOrDefault(t => t.Name == homeName);
                if (home == null)
                {
                    home = new TeamResult { Name = homeName };
                    TeamResults.Add(home);
                }

                var away = TeamResults.FirstOrDefault(t => t.Name == awayName);
                if (away == null)
                {
                    away = new TeamResult { Name = awayName };
                    TeamResults.Add(away);
                }

                if (match.Home_score.HasValue && match.Away_score.HasValue)
                {
                    int hs = match.Home_score.Value;
                    int ascore = match.Away_score.Value;

                    home.NbOfGamePlayed += 1;
                    home.GameWon = (home.GameWon ?? 0) + hs;
                    home.GameLost = (home.GameLost ?? 0) + ascore;
                    home.GameAverage = (home.GameAverage ?? 0) + (hs - ascore);

                    away.NbOfGamePlayed += 1;
                    away.GameWon = (away.GameWon ?? 0) + ascore;
                    away.GameLost = (away.GameLost ?? 0) + hs;
                    away.GameAverage = (away.GameAverage ?? 0) + (ascore - hs);

                    if (hs > ascore)
                    {
                        home.Victory += 1;
                        home.Points += 3;
                        away.Defeat += 1;
                        away.Points += 1;
                    }
                    else if (hs < ascore)
                    {
                        away.Victory += 1;
                        away.Points += 3;
                        home.Defeat += 1;
                        home.Points += 1;
                    }
                    else
                    {
                        home.Draw += 1;
                        away.Draw += 1;
                        home.Points += 2;
                        away.Points += 2;
                    }
                }
            }

            // tri final
            TeamResults = TeamResults
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GameAverage ?? 0)
                .ToList();
        }
    }
}
