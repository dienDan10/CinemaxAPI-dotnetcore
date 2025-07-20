namespace CinemaxAPI.Models.DTO.Requests
{
    public class GetRevenueRequestDTO
    {

        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public int? TheaterId { get; set; } = null;
    }
}
