namespace CinemaxAPI.Models.DTO.Responses
{
    public class ErrorResponseDTO
    {
        public string Message { get; set; }
        public string Errors { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
    }
}
