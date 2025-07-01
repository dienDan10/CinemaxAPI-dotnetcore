namespace CinemaxAPI.Models.DTO
{
    public class ManagerDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsLocked { get; set; } = false;

        public TheaterDTO Theater { get; set; }
    }
}
