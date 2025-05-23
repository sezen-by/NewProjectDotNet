using System;
using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; } // HASHLEEEEEEEEEEEEE
        [Required]
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
