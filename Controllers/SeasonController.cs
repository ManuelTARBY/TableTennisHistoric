using Microsoft.AspNetCore.Mvc;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SeasonController : Controller
    {
        private readonly TableTennisHistoricDbContext _context;

        public SeasonController(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var season = _context.Season.FirstOrDefault(p => p.Id == id);
            return Ok(season);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var seasons = _context.Season.ToList();
            return Ok(seasons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSeasonAutomatically()
        {

            DateOnly today = DateOnly.FromDateTime(DateTime.Now.AddYears(1));
            DateOnly start;
            DateOnly end;

            if (today.Month >= 9)
            {
                start = new DateOnly(today.Year, 9, 1);
                end = new DateOnly(today.Year + 1, 8, 31);
            }
            else
            {
                start = new DateOnly(today.Year - 1, 9, 1);
                end = new DateOnly(today.Year, 8, 31);
            }

            var season = new Season
            {
                Start_date = start,
                End_date = end
            };
            season.Name = $"Saison {start.Year}-{end.Year}";

            _context.Season.Add(season);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }

}
