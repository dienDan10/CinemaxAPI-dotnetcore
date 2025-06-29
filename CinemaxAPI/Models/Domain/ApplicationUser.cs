using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaxAPI.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;
        [NotMapped]
        public string[]? Roles { get; set; }
        
        // Navigation property for the theater this user manages (if user is a manager)
        public Theater? ManagedTheater { get; set; }
        
        // Navigation property for the theater this user works in (if user is an employee)
        public int? TheaterId { get; set; }
        [ForeignKey("TheaterId")]
        public Theater? EmployedTheater { get; set; }
    }
}
