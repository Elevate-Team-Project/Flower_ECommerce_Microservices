using Microsoft.AspNetCore.Identity;

namespace Auth.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? ProfileImageUrl { get; set; }
    
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string FullName { get; set; }
 
        public int Age { get; set; }
        public string Gender { get; set; }

     


        // 🔁 Refresh Token Support
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
