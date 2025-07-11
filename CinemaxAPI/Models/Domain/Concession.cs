using System.ComponentModel.DataAnnotations;

namespace CinemaxAPI.Models.Domain
{
    public class Concession
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsRemoved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
