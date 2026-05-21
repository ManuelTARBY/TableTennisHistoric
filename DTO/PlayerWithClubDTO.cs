using TableTennisHistoric.Models;

namespace TableTennisHistoric.DTO
{
    public class PlayerWithClubDTO
    {
        public Player Player { get; set; } = null!;
        public Club? Club { get; set; }
    }
}
