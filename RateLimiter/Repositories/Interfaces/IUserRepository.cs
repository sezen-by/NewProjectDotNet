using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UserExistsAsync(string username);
    }
} 