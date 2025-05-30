using RateLimiter.Models;

namespace RateLimiter.Interfaces
{
    public interface IWhitelistRepository
    {
        Task<List<WhitelistedUser>> GetAllWhitelistedUsersAsync();
        Task<WhitelistedUser?> GetWhitelistedUserByUserIdAsync(int userId);
        Task<WhitelistedUser?> GetWhitelistedUserByUsernameAsync(string username);
        Task<bool> AddToWhitelistAsync(WhitelistedUser whitelistedUser);
        Task<bool> UpdateWhitelistedUserAsync(WhitelistedUser whitelistedUser);
        Task<bool> RemoveFromWhitelistAsync(string username);
        Task<bool> IsUserWhitelistedAsync(int userId);
    }
} 