namespace CinemaxAPI.Models.DTO
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? BookingStatus { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
