namespace CinemaxAPI.Models.DTO.Requests
{
    public class BookingHistoryFilterRequest
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
