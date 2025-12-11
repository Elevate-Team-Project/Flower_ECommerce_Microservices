using Microsoft.AspNetCore.Identity;

namespace Auth.Models
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}
