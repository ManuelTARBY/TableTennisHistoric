using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TableTennisHistoric.Pages
{
    public class PointsCalculatorModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();
        public SelectList Coefficients { get; set; } = new SelectList(
                new[]
                {
                    new { Value = 0.5m, Text = "0.5 (Tournois régionaux et départementaux)" },
                    new { Value = 0.75m, Text = "0.75 (Tournois nationaux B)" },
                    new { Value = 1.0m, Text = "1.0 (Championnat par équipe, Tournois nationaux A et internationaux)" },
                    new { Value = 1.25m, Text = "1.25 (Finales par classement)"},
                    new { Value = 1.5m, Text = "1.5 (Critérium fédéral, Finales individuelles)" },
                    new { Value = 2.0m, Text = "2.0 (Championnat de France senior)" }
                },
                "Value",
                "Text",
                1.0m // valeur sélectionnée
            );

        public decimal? ResultPoints { get; set; }

        private static readonly (decimal minDiff, decimal gain)[] gainTableVictory =
            {
                (500, 40),
                (400, 28),
                (300, 22),
                (200, 17),
                (150, 13),
                (100, 10),
                (50, 8),
                (25, 7),
                (-24.99m, 6),
                (-49.99m, 5.5m),
                (-99.99m, 5),
                (-149.99m, 4),
                (-199.99m, 3),
                (-299.99m, 2),
                (-399.99m, 1),
                (-499.99m, 0.5m),
                (-10000, 0)
            };
        private static readonly (decimal minDiff, decimal gain)[] gainTableDefeat =
            {
                (400, 0),
                (300, -0.5m),
                (200, -1),
                (150, -2),
                (100, -3),
                (50, -4),
                (25, -4.5m),
                (-24.99m, -5),
                (-49.99m, -6),
                (-99.99m, -7),
                (-149.99m, -8),
                (-199.99m, -10),
                (-299.99m, -12.5m),
                (-399.99m, -16),
                (-499.99m, -20),
                (-10000, -29)
            };
        public enum MatchResult
        {
            Victory,
            Defeat
        }

        public class InputModel
        {
            public decimal MyPoints { get; set; } = 500.00m;
            public decimal OpponentPoints { get; set; } = 500.00m;
            public decimal Coefficient { get; set; } = 1.0m;
            public MatchResult Result { get; set; }
        }

        public void OnGet()
        {
            ModelState.Clear();
        }

        public void OnPost()
        {
            ModelState.Clear();
            ResultPoints = Math.Round(Compute(), 2);
        }

        public decimal Compute()
        {

            if (Input.Coefficient == null) { return 0; }

            decimal diff = Input.OpponentPoints - Input.MyPoints;

            var table = Input.Result switch
            {
                MatchResult.Victory => gainTableVictory,
                MatchResult.Defeat => gainTableDefeat,
                _ => Array.Empty<(decimal, decimal)>()
            };

            foreach (var (minDiff, gain) in table)
            {
                if (diff >= minDiff)
                    return gain * Input.Coefficient;
            }
            return 0;
        }
    }
}
