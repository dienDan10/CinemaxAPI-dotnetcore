namespace CinemaxAPI.Models.DTO.Requests
{
    public class EmployeeFilterRequestDTO
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? TheaterId { get; set; }
    }
}
