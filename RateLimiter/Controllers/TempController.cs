using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TempController : ControllerBase
    {
        /// <summary>
        /// Geçici veri döner. Admin kullanıcılar için rate limit yoktur, diğer kullanıcılar için 10 saniyede 3 istek sınırı vardır.
        /// </summary>
        /// <returns>Geçici veri ve zaman damgası</returns>
        [HttpGet]
        [Authorize]
        public IActionResult GetTempData()
        {
            return Ok(new {
                message = "Bu geçici bir veridir.",
                timestamp = DateTime.UtcNow,
                user = User.Identity?.Name,
                isAdmin = User.IsInRole("admin")
            });
        }
    }
}
