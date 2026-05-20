using Microsoft.AspNetCore.Mvc;
using TableTennisHistoric.Datas;
using TableTennisHistoric.Models;

namespace TableTennisHistoric.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ClubController : Controller
    {
        private readonly TableTennisHistoricDbContext _context;

        public ClubController(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var club = _context.Club.FirstOrDefault(p => p.Id == id);
            return Ok(club);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var clubs = _context.Club.ToList();
            return Ok(clubs);
        }
    }

}
