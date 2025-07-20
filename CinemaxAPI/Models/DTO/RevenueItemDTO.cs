namespace CinemaxAPI.Models.DTO
{
    public class RevenueItemDTO
    {
        public DateOnly Date { get; set; }
        public PaymentConciseDTO Payment { get; set; }
        public MovieConciseDTO Movie { get; set; }
    }

    public class PaymentConciseDTO
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public int TicketCount { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class MovieConciseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
