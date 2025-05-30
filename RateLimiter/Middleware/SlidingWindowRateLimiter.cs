using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.Extensions.Options;
using RateLimiter.Configuration;

namespace RateLimiter.Middleware
{
    public class SlidingWindowRateLimiter
    {
        private readonly RequestDelegate _next;  // Pipeline'daki bir sonraki middleware
        private readonly IMemoryCache _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SlidingWindowRateLimiter> _logger;
        private readonly int _maxRequests;
        private readonly TimeSpan _window;

        private readonly string[] _excludedPaths = {
            "/api/auth/login",
            "/api/auth/register",
            "/swagger" 
        };

        public SlidingWindowRateLimiter(
            RequestDelegate next,
            IMemoryCache cache,
            IServiceProvider serviceProvider,
            ILogger<SlidingWindowRateLimiter> logger,
            IOptions<RateLimitingOptions> options)
        {
            _next = next;
            _cache = cache;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _maxRequests = options.Value.MaxRequests;
            _window = TimeSpan.FromSeconds(options.Value.WindowSeconds);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                _logger.LogInformation($"Skipping rate limit for endpoint with [AllowAnonymous]: {path}");
                await _next(context);
                return;
            }

            if (_excludedPaths.Any(excludedPath => path.StartsWith(excludedPath)))
            {
                _logger.LogInformation($"Skipping rate limit for excluded path: {path}");
                await _next(context);
                return;
            }

            foreach (var claim in context.User.Claims)
            {
                _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation($"UserId from NameIdentifier: {userId}");

            if (userId == null)
            {
                _logger.LogInformation("Skipping rate limit for anonymous/public user");
                await _next(context);
                return;
            }

            string clientId = userId;
            _logger.LogInformation($"ClientId: {clientId}");

            bool isWhitelisted = false;
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                isWhitelisted = await dbContext.WhitelistedUsers
                    .AsNoTracking()
                    .AnyAsync(w => w.UserId == int.Parse(userId) && w.IsActive);
                _logger.LogInformation($"Database whitelist check result: {isWhitelisted}");
            }

            if (isWhitelisted)
            {
                _logger.LogInformation("Skipping rate limit for whitelisted user");
                await _next(context);
                return;
            }

            string cacheKey = $"rate_limit_{clientId}";
            _logger.LogInformation($"Cache key: {cacheKey}");

            var requestLog = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = _window;
                return new ConcurrentQueue<DateTime>();
            });

            var now = DateTime.UtcNow;
            while (requestLog!.TryPeek(out DateTime oldestRequest) &&
                  now - oldestRequest > _window)
            {
                requestLog.TryDequeue(out _);
            }

            _logger.LogInformation($"Current request count: {requestLog.Count}");

            if (requestLog.Count >= _maxRequests)
            {
                _logger.LogWarning($"Rate limit exceeded for client {clientId}");
                var retryAfterSeconds = (int)_window.TotalSeconds;
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    message = $"Rate limit exceeded. Try again in {retryAfterSeconds} seconds.",
                    retryAfter = _window.TotalSeconds,
                    currentCount = requestLog.Count,
                    maxRequests = _maxRequests
                });
                return;
            }

            requestLog.Enqueue(now);
            _cache.Set(cacheKey, requestLog, _window);

            context.Response.Headers.Append("X-RateLimit-Limit", _maxRequests.ToString());
            context.Response.Headers.Append("X-RateLimit-Remaining", (_maxRequests - requestLog.Count).ToString());
            var resetTime = now.Add(_window);
            context.Response.Headers.Append("X-RateLimit-Reset", new DateTimeOffset(resetTime).ToUnixTimeSeconds().ToString());

            await _next(context);
        }
    }

    public static class SlidingWindowRateLimiterExtensions
    {
        public static IApplicationBuilder UseSlidingWindowRateLimiter(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SlidingWindowRateLimiter>();
        }
    }
} 