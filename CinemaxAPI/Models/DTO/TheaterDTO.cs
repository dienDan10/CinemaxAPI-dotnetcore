using CinemaxAPI.Models.Domain;

namespace CinemaxAPI.Models.DTO
{
    public class TheaterDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public Province Province { get; set; }

    }
}
