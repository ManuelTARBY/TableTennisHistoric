namespace TableTennisHistoric.DTO
{
    public class PlayerEditDTO
    {
        public int Id { get; set; }
        public string First_name { get; set; } = "";
        public string Last_name { get; set; } = "";
        public string? License_number { get; set; }
    }
}