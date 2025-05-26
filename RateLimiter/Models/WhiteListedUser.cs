using System.ComponentModel.DataAnnotations;

namespace RateLimiter.Models
{
    public class WhitelistedUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty; // Performans için username'i de tutuyoruz
        public string Description { get; set; } = string.Empty; // Neden whitelist'e alındığını açıklamak için
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true; // Geçici olarak devre dışı bırakabilmek için
        
        // Navigation property
        public User? User { get; set; }
    }
}