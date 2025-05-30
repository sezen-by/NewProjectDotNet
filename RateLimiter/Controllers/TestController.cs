using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.Services.Interfaces;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Authorize] 
    public class TestController : ControllerBase
    {
        private readonly ITestService _testService;

        public TestController(ITestService testService)
        {
            _testService = testService;
        }

        [HttpGet("rate-limit")]
        public IActionResult TestRateLimit()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
            
            var response = _testService.ProcessAuthenticatedRequest(userId, username);
            return Ok(response);
        }
    }
} 