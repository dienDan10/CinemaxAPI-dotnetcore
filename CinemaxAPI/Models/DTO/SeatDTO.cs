namespace CinemaxAPI.Models.DTO
{
    public class SeatDTO
    {
        public int Id { get; set; }
        public string SeatRow { get; set; }
        public int SeatNumber { get; set; }
        public int ScreenId { get; set; }
        public bool IsActive { get; set; }
    }
}
