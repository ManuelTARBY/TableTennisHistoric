namespace TableTennisHistoric.Models
{
    public class ChampionshipFormat
    {
        public int Id { get; set; }
        public int Nb_of_team { get; set; }
        public int Matchday { get; set; }
        public string Home { get; set; }
        public string Away { get; set; }
    }
}
