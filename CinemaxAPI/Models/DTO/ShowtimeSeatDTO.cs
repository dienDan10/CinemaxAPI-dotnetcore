namespace CinemaxAPI.Models.DTO
{
    public class ShowtimeSeatDTO
    {
        public int Id { get; set; }
        public string SeatRow { get; set; }
        public int SeatNumber { get; set; }
        public bool IsBooked { get; set; }
    }
}
