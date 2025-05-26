using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Sadece giriş yapmış kullanıcılar için
    public class TestController : ControllerBase
    {
        private static int _requestCounter = 0;

        [HttpGet("test-rate-limit")]
        public IActionResult TestRateLimit()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            _requestCounter++;

            return Ok(new
            {
                message = "Test endpoint başarıyla çağrıldı!",
                requestNumber = _requestCounter,
                timestamp = DateTime.UtcNow,
                userId = userId,
                username = username
            });
        }

        [HttpGet("public-test")]
        [AllowAnonymous] // Herkes erişebilir
        public IActionResult PublicTest()
        {
            _requestCounter++;

            return Ok(new
            {
                message = "Public test endpoint başarıyla çağrıldı!",
                requestNumber = _requestCounter,
                timestamp = DateTime.UtcNow,
                isAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }
    }
} 