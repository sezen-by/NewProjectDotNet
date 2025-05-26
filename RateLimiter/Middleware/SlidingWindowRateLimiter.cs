using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RateLimiter.Middleware
{
    public class SlidingWindowRateLimiter
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SlidingWindowRateLimiter> _logger;
        private readonly int _maxRequests;
        private readonly TimeSpan _window;

        public SlidingWindowRateLimiter(
            RequestDelegate next,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            ILogger<SlidingWindowRateLimiter> logger,
            int maxRequests = 100,  // Default 100 requests
            int windowSeconds = 60)  // Default 1 minute window
        {
            _next = next;
            _cache = cache;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _maxRequests = maxRequests;
            _window = TimeSpan.FromSeconds(windowSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Tüm claim'leri logla
            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"UserId from NameIdentifier: {userId}");

            var isWhitelistedClaim = context.User.FindFirst("isWhitelisted")?.Value;
            _logger.LogInformation($"isWhitelisted from claim: {isWhitelistedClaim}");

            // If user is not authenticated, use IP address as identifier
            string clientId = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogInformation($"ClientId: {clientId}");

            // Check if user is whitelisted
            bool isWhitelisted = false;

            // Önce veritabanından kontrol et
            if (userId != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    isWhitelisted = await dbContext.WhitelistedUsers
                        .AsNoTracking()
                        .AnyAsync(w => w.UserId == int.Parse(userId) && w.IsActive);
                    _logger.LogInformation($"Database whitelist check result: {isWhitelisted}");
                }
            }

            // Eğer veritabanında yoksa claim'i kontrol et
            if (!isWhitelisted && isWhitelistedClaim?.ToLower() == "true")
            {
                isWhitelisted = true;
                _logger.LogInformation("User is whitelisted based on JWT claim");
            }

            _logger.LogInformation($"Final whitelist status: {isWhitelisted}");

            // If user is whitelisted, skip rate limiting
            if (isWhitelisted)
            {
                _logger.LogInformation("Skipping rate limit for whitelisted user");
                await _next(context);
                return;
            }

            string cacheKey = $"rate_limit_{clientId}";
            _logger.LogInformation($"Cache key: {cacheKey}");

            // Get or create the request log for this client
            var requestLog = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = _window;
                return new ConcurrentQueue<DateTime>();
            });

            // Remove old requests outside the window
            var now = DateTime.UtcNow;
            while (requestLog!.TryPeek(out DateTime oldestRequest) &&
                  now - oldestRequest > _window)
            {
                requestLog.TryDequeue(out _);
            }

            _logger.LogInformation($"Current request count: {requestLog.Count}");

            // Check if the request count is within limits
            if (requestLog.Count >= _maxRequests)
            {
                _logger.LogWarning($"Rate limit exceeded for client {clientId}");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    retryAfter = _window.TotalSeconds,
                    currentCount = requestLog.Count,
                    maxRequests = _maxRequests
                });
                return;
            }

            // Add current request timestamp
            requestLog.Enqueue(now);

            // Add rate limit headers using Append instead of Add
            context.Response.Headers.Append("X-RateLimit-Limit", _maxRequests.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", (_maxRequests - requestLog.Count).ToString());
            context.Response.Headers.Append("X-RateLimit-Reset", ((int)(now + _window).Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString());

            await _next(context);
        }
    }

    // Extension method for easy middleware registration
    public static class SlidingWindowRateLimiterExtensions
    {
        public static IApplicationBuilder UseSlidingWindowRateLimiter(
            this IApplicationBuilder builder,
            int maxRequests = 100,
            int windowSeconds = 60)
        {
            return builder.UseMiddleware<SlidingWindowRateLimiter>(maxRequests, windowSeconds);
        }
    }
} 
} 