using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Models
{
    public class WhitelistedUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true; 
        
        public User? User { get; set; }
    }
}