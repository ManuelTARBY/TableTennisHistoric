using Microsoft.AspNetCore.Mvc;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CompetitionController : Controller
    {
        private readonly TableTennisHistoricDbContext _context;

        public CompetitionController(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var competition = _context.Competition.FirstOrDefault(p => p.Id == id);
            return Ok(competition);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var competitions = _context.Competition.ToList();
            return Ok(competitions);
        }
    }

}
