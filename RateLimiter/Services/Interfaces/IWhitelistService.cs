using RateLimiter.Models.DTOs;

namespace RateLimiter.Services.Interfaces
{
    public interface IWhitelistService
    {
        Task<List<WhitelistedUserDto>> GetAllWhitelistedUsersAsync();
        Task<AddToWhitelistResponse> AddToWhitelistAsync(AddToWhitelistRequest request);
        Task<RemoveFromWhitelistResponse> RemoveFromWhitelistAsync(string username);
    }
} 