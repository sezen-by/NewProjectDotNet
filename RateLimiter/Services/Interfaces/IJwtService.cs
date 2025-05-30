using RateLimiter.Models;

namespace RateLimiter.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
    }
} 