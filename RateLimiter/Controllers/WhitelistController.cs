using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateLimiter.Models.DTOs;
using RateLimiter.Services.Interfaces;
using System.Security.Claims;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/whitelist")]
    [Authorize] 
    public class WhitelistController : ControllerBase
    {
        private readonly IWhitelistService _whitelistService;

        public WhitelistController(IWhitelistService whitelistService)
        {
            _whitelistService = whitelistService;
        }

        // Whitelist'teki tüm kullanıcıları getir (sadece admin)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetWhitelistedUsers()
        {
            try
            {
                var whitelistedUsers = await _whitelistService.GetAllWhitelistedUsersAsync();
                return Ok(whitelistedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Kullanıcıyı whitelist'e ekle (sadece admin)
        [HttpPost("add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddToWhitelist([FromBody] AddToWhitelistRequest request)
        {
            try
            {
                var result = await _whitelistService.AddToWhitelistAsync(request);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        // Kullanıcıyı whitelist'ten çıkar (sadece admin)
        [HttpDelete("remove/{username}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveFromWhitelist(string username)
        {
            try
            {
                var result = await _whitelistService.RemoveFromWhitelistAsync(username);

                if (!result.Success)
                    return NotFound(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}