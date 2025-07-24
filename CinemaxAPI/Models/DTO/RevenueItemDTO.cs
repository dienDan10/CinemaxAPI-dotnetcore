namespace CinemaxAPI.Models.DTO
{
    public class RevenueItemDTO
    {
        public DateOnly Date { get; set; }
        public PaymentConciseDTO Payment { get; set; } = new PaymentConciseDTO();
        public TheaterConciseDTO Theater { get; set; } = new TheaterConciseDTO();
        public MovieConciseDTO Movie { get; set; } = new MovieConciseDTO();
        public ShowtimeConciseDTO ShowTime { get; set; } = new ShowtimeConciseDTO();
        public List<ConcessionConciseDTO> Concessions { get; set; } = new List<ConcessionConciseDTO>();
    }

    public class PaymentConciseDTO
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentStatus { get; set; }
    }

    public class TheaterConciseDTO
    {
        public int Id { get; set; }
        public string? TheaterName { get; set; }
        public decimal Amount { get; set; }
    }

    public class ShowtimeConciseDTO
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int TicketCount { get; set; }
        public decimal Amount { get; set; }
    }

    public class MovieConciseDTO
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int TicketCount { get; set; }
        public decimal Amount { get; set; }
    }

    public class ConcessionConciseDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
