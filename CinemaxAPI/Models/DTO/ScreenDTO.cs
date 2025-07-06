namespace CinemaxAPI.Models.DTO
{
    public class ScreenDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TheaterName { get; set; }

        public int Rows { get; set; }

        public int Columns { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
