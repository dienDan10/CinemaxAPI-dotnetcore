namespace CinemaxAPI.Models.DTO.Responses
{
    public class SuccessResponseDTO
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public int StatusCode { get; set; } = 200;
        public object? Data { get; set; }
    }
}
