namespace TableTennisHistoric.DTO
{
    public class PlayerDTO
    {
        public string? License_number { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public Boolean Is_me { get; set; }
        public string Category { get; set; }
        public List<ClubDTO> Clubs { get; set; }
    }
}
