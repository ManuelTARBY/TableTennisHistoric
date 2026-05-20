using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableTennisHistoric.Datas;
using TableTennisHistoric.DTO;

namespace TableTennisHistoric.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PlayerController : Controller
    {
        private readonly TableTennisHistoricDbContext _context;

        public PlayerController(TableTennisHistoricDbContext context)
        {
            _context = context;
        }

        [HttpGet("myspace")]
        public IActionResult? MySpace()
        {

            var player = _context.Player
            .Include(p => p.PlayerSeasons)
            .ThenInclude(pc => pc.Club)
            .FirstOrDefault(p => p.Is_me == true);

            if (player == null)
            {
                return null;
            }

            var playerDto = new PlayerDTO
            {
                First_name = player.First_name,
                Last_name = player.Last_name,
                Is_me = player.Is_me,
                Clubs = player.PlayerSeasons
                  .Select(pc => new ClubDTO
                  {
                      License_number = pc.Club.License_number,
                      Name = pc.Club.Name,
                      City = pc.Club.City
                  })
                  .ToList()
            };

            return Ok(playerDto);
        }

        [HttpGet("{id}")]
        public IActionResult? GetById(int id)
        {

            var player = _context.Player
            .Include(p => p.PlayerSeasons)
            .ThenInclude(pc => pc.Club)
            .FirstOrDefault(p => p.Id == id);

            if (player == null)
            {
                return null;
            }

            var playerDto = new PlayerDTO
            {
                First_name = player.First_name,
                Last_name = player.Last_name,
                Is_me = player.Is_me,
                Clubs = player.PlayerSeasons
                  .Select(pc => new ClubDTO
                  {
                      License_number = pc.Club.License_number,
                      Name = pc.Club.Name,
                      City = pc.Club.City
                  })
                  .ToList()
            };

            return Ok(playerDto);
        }

        [HttpGet("all")]
        public IActionResult All()
        {
            var players = _context.Player.ToList();
            return Ok(players);
        }
    }

}
