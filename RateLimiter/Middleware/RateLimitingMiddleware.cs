using System.Collections.Concurrent;
using System.Security.Claims;

namespace RateLimiter.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, RequestCounter> _requests = new();
        private static readonly TimeSpan _timeWindow = TimeSpan.FromSeconds(10);
        private const int LIMIT = 3;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            if (!path.Contains("/temp")) // sadece /temp endpointine uygula
            {
                await _next(context);
                return;
            }

            var user = context.User;
            if (user.Identity?.IsAuthenticated == true && user.IsInRole("admin"))
            {
                await _next(context); // admin kullanıcıya sınır yok
                return;
            }

            string key = user.Identity?.IsAuthenticated == true
                ? $"user:{user.Identity.Name}"
                : $"ip:{context.Connection.RemoteIpAddress}";

            var counter = _requests.GetOrAdd(key, _ => new RequestCounter
            {
                Timestamp = DateTime.UtcNow,
                Count = 0
            });

            lock (counter)
            {
                if (DateTime.UtcNow - counter.Timestamp > _timeWindow)
                {
                    counter.Timestamp = DateTime.UtcNow;
                    counter.Count = 1;
                }
                else
                {
                    counter.Count++;
                }

                if (counter.Count > LIMIT)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = "10";
                    return;
                }
            }

            await _next(context);
        }

        private class RequestCounter
        {
            public DateTime Timestamp { get; set; }
            public int Count { get; set; }
        }
    }
}
