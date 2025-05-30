using RateLimiter.Models.DTOs;
using RateLimiter.Services.Interfaces;
using System.Collections.Concurrent;

namespace RateLimiter.Services
{
    public class TestService : ITestService
    {
        // Sadece kullanıcı bazlı counter'lar
        private static readonly ConcurrentDictionary<string, int> _userRequestCounters = new();

        public TestResponseDto ProcessAuthenticatedRequest(string? userId, string? username)
        {
            var userKey = userId ?? "anonymous";
            
            // Kullanıcı bazlı counter'ı artır
            var userRequestCount = _userRequestCounters.AddOrUpdate(userKey, 1, (key, oldValue) => oldValue + 1);

            return new TestResponseDto
            {
                Message = "Test endpoint başarıyla çağrıldı!",
                RequestNumber = userRequestCount
            };
        }
    }
} 