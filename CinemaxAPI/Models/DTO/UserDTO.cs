namespace CinemaxAPI.Models.DTO
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsLocked { get; set; } = true;
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
