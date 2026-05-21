using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TableTennisHistoric.Services.Interfaces;

namespace TableTennisHistoric.Pages
{
    public class PointsCalculatorModel : PageModel
    {
        private readonly IMatchService _matchService;

        public PointsCalculatorModel(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList Coefficients { get; set; } = new SelectList(
            new[]
            {
                new { Value = 0.5m,  Text = "0.5 (Tournois régionaux et départementaux)" },
                new { Value = 0.75m, Text = "0.75 (Tournois nationaux B)" },
                new { Value = 1.0m,  Text = "1.0 (Championnat par équipe, Tournois nationaux A et internationaux)" },
                new { Value = 1.25m, Text = "1.25 (Finales par classement)" },
                new { Value = 1.5m,  Text = "1.5 (Critérium fédéral, Finales individuelles)" },
                new { Value = 2.0m,  Text = "2.0 (Championnat de France senior)" }
            },
            "Value", "Text", 1.0m);

        public decimal? ResultPoints { get; set; }

        public enum MatchResult { Victory, Defeat }

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
            ResultPoints = Math.Round(_matchService.ComputeFromCalculator(
                Input.MyPoints,
                Input.OpponentPoints,
                Input.Coefficient,
                Input.Result == MatchResult.Victory
            ), 2);
        }
    }
}