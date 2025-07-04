namespace CinemaxAPI.Models.DTO.Requests
{
    public class SortRequestDTO
    {
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
    }
}
