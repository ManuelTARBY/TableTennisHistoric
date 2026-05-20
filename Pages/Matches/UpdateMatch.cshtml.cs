using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using System.ComponentModel.DataAnnotations;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Pages.Matches
{
    public class UpdateMatchModel : PageModel
    {
        private readonly TableTennisHistoricDbContext _context;

        public UpdateMatchModel(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        public List<SelectListItem> CompetitionCoefficients { get; set; } = new();
        public List<SelectListItem> Opponents { get; set; } = new();
        public IEnumerable<SelectListItem> Stages { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            public DateTime Date_match { get; set; }

            [Required]
            public int CompetitionCoefficientId { get; set; }

            [Required]
            public int OpponentId { get; set; }
            public int? StageId { get; set; } = null;

            public decimal My_points_at_match { get; set; }
            public decimal Opponent_points_at_match { get; set; }

            [Required]
            public TableTennisMatch.MatchResult Result { get; set; }

            public string? Comment { get; set; }

            public List<SetInputModel> Sets { get; set; } = new();
        }

        public class SetInputModel
        {
            public int SetNumber { get; set; }
            public int Player1Score { get; set; }
            public int Player2Score { get; set; }
        }

        // GET
        public async Task<IActionResult> OnGetAsync(int id)
        {
            //await LoadSelectListsAsync();

            var match = await _context.TableTennisMatch
                .Include(m => m.Sets)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
                return NotFound();

            await LoadSelectListsAsync(match);

            Input = new InputModel
            {
                Id = match.Id,
                Date_match = match.Date_match.ToDateTime(TimeOnly.MinValue),
                CompetitionCoefficientId = match.CompetitionCoefficientId,
                StageId = match.StageId,
                OpponentId = match.OpponentId,
                My_points_at_match = match.My_points_at_match,
                Opponent_points_at_match = match.Opponent_points_at_match,
                Result = match.Result,
                Comment = match.Comment,
                Sets = match.Sets?
                    .OrderBy(s => s.SetNumber)
                    .Select(s => new SetInputModel
                    {
                        SetNumber = s.SetNumber,
                        Player1Score = s.Player1Score,
                        Player2Score = s.Player2Score
                    }).ToList() ?? new List<SetInputModel>()
            };

            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            //await LoadSelectListsAsync();

            if (!ModelState.IsValid)
                return Page();

            var match = await _context.TableTennisMatch
                .Include(m => m.Sets)
                .FirstOrDefaultAsync(m => m.Id == Input.Id);

            if (match == null)
                return NotFound();

            await LoadSelectListsAsync(match);

            // Filtrage des sets vides
            Input.Sets = Input.Sets
                .Where(s => (s.Player1Score > 0 || s.Player2Score > 0) && (s.Player1Score >= 11 || s.Player2Score >= 11))
                .ToList();

            // Validation des sets
            if (Input.Sets.Count >= 3)
            {
                var setErrors = ValidateSets(Input.Sets, Input.Result);
                foreach (var error in setErrors)
                    ModelState.AddModelError(string.Empty, error);

                if (!ModelState.IsValid)
                    return Page();
            }

            // Update match
            match.Date_match = DateOnly.FromDateTime(Input.Date_match);
            match.CompetitionCoefficientId = Input.CompetitionCoefficientId;
            match.StageId = Input.StageId;
            match.OpponentId = Input.OpponentId;
            match.My_points_at_match = Input.My_points_at_match;
            match.Opponent_points_at_match = Input.Opponent_points_at_match;
            match.Result = Input.Result;
            match.Comment = Input.Comment;

            // S'il y a moins de 3 sets valables, on n'enregistre que le match sans les sets
            if (Input.Sets.Count < 3)
            {
                await _context.SaveChangesAsync();

                return RedirectToPage("/Index");
            }

            // Update sets (simple version : delete + recreate)
            _context.MatchSet.RemoveRange(match.Sets ?? Enumerable.Empty<MatchSet>());

            match.Sets = Input.Sets.Select(s => new MatchSet
            {
                SetNumber = s.SetNumber,
                Player1Score = s.Player1Score,
                Player2Score = s.Player2Score
            }).ToList();

            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }

        private async Task LoadSelectListsAsync(TableTennisMatch match)
        {
            //var today = DateOnly.FromDateTime(DateTime.Today);

            var currentSeason = await _context.Season
                .FirstOrDefaultAsync(s => s.Start_date <= match.Date_match && s.End_date >= match.Date_match);
            //var currentSeason = await _context.Season
            //    .FirstOrDefaultAsync(s => s.Start_date <= today && s.End_date >= today);

            if (currentSeason == null) return;

            CompetitionCoefficients = await _context.CompetitionCoefficient
                .Include(cc => cc.Competition)
                .Where(cc => cc.SeasonId == currentSeason.Id)
                .Select(cc => new SelectListItem
                {
                    Value = cc.Id.ToString(),
                    Text = cc.Competition.Name
                }).ToListAsync();

            Opponents = await _context.Player
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.First_name + " " + p.Last_name
                })
                .ToListAsync();

            Opponents = Opponents.OrderBy(o => o.Text).ToList();

            Stages = await _context.Stage
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();
        }

        /**
         * Validation des sets selon les règles du tennis de table : 11 points minimum pour le gagnant, écart de 2 points minimum, et cohérence avec le résultat du match.
         * 
         */
        private List<string> ValidateSets(List<SetInputModel> sets, TableTennisMatch.MatchResult result)
        {
            var errors = new List<string>();

            foreach (var set in sets)
            {
                int max = Math.Max(set.Player1Score, set.Player2Score);
                int min = Math.Min(set.Player1Score, set.Player2Score);
                int diff = max - min;

                // Règle 1 : gagné à 11 points minimum
                if (max < 11)
                    errors.Add($"Set {set.SetNumber} : le gagnant doit avoir au moins 11 points ({set.Player1Score}-{set.Player2Score}).");

                // Règle 2 : écart minimum de 2 points
                if (diff < 2)
                    errors.Add($"Set {set.SetNumber} : l'écart entre les joueurs doit être d'au moins 2 points ({set.Player1Score}-{set.Player2Score}).");
            }

            // Règle 3 : le vainqueur du match doit avoir gagné la majorité des sets
            if (result != TableTennisMatch.MatchResult.F) // on ignore les forfaits
            {
                int setsPlayer1 = sets.Count(s => s.Player1Score > s.Player2Score);
                int setsPlayer2 = sets.Count(s => s.Player2Score > s.Player1Score);

                bool player1Won = result == TableTennisMatch.MatchResult.V;

                if (player1Won && setsPlayer1 < setsPlayer2)
                    errors.Add("Le résultat indique une victoire, mais l'adversaire a gagné plus de sets.");

                if (player1Won && setsPlayer2 == setsPlayer1)
                    errors.Add("Le résultat indique une victoire, mais vous avez gagné autant de sets que l'adversaire.");

                if (!player1Won && setsPlayer1 == setsPlayer2)
                    errors.Add("Le résultat indique une défaite, mais vous avez gagné autant de sets que l'adversaire.");

                if (!player1Won && setsPlayer2 < setsPlayer1)
                    errors.Add("Le résultat indique une défaite, mais vous avez gagné plus de sets.");
            }

            return errors;
        }
    }
}