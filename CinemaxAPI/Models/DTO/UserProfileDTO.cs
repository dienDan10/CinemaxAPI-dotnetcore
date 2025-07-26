namespace CinemaxAPI.Models.DTO
{
    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public int? TheaterId { get; set; }
        public string Role { get; set; }
        public int Point { get; set; }
    }
}
