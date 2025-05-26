using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.user; // Default role
        public DateTime CreatedAt { get; set; }
    }
}