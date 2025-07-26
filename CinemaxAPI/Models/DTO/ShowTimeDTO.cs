namespace CinemaxAPI.Models.DTO
{
    public class ShowTimeDTO
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int ScreenId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal VipTicketPrice { get; set; }
        public bool IsActive { get; set; }
    }
}
