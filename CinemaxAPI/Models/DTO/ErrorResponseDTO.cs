namespace CinemaxAPI.Models.DTO
{
    public class ErrorResponseDTO
    {
        public string Message { get; set; }
        public string Errors { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
    }
}
