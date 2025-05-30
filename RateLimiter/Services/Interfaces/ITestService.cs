using RateLimiter.Models.DTOs;

namespace RateLimiter.Services.Interfaces
{
    public interface ITestService
    {
        TestResponseDto ProcessAuthenticatedRequest(string? userId, string? username);
    }
} 