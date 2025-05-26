using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using RateLimiter.Models;
using System.Security.Claims;

namespace RateLimiter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Giriş yapmış kullanıcılar için
    public class WhitelistController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WhitelistController(AppDbContext context)
        {
            _context = context;
        }

        // Whitelist'teki tüm kullanıcıları getir (sadece admin)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetWhitelistedUsers()
        {
            try
            {
                var whitelistedUsers = await _context.WhitelistedUsers
                    .Include(w => w.User)
                    .AsNoTracking()
                    .Select(w => new
                    {
                        w.Id,
                        w.UserId,
                        w.Username,
                        w.Description,
                        w.CreatedAt,
                        w.IsActive,
                        UserRole = w.User != null ? w.User.Role.ToString() : "Unknown"
                    })
                    .ToListAsync();

                return Ok(whitelistedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Kullanıcıyı whitelist'e ekle (sadece admin)
        [HttpPost("add")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddToWhitelist([FromBody] AddToWhitelistRequest request)
        {
            try
            {
                // Kullanıcının var olup olmadığını kontrol et
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                    return NotFound("User not found");

                // Zaten whitelist'te var mı kontrol et
                var existingWhitelistEntry = await _context.WhitelistedUsers
                    .FirstOrDefaultAsync(w => w.UserId == user.Id);

                if (existingWhitelistEntry != null)
                {
                    if (existingWhitelistEntry.IsActive)
                        return BadRequest("User is already whitelisted");
                    
                    // Eğer deaktif durumda ise aktif yap
                    existingWhitelistEntry.IsActive = true;
                    existingWhitelistEntry.Description = request.Description ?? existingWhitelistEntry.Description;
                }
                else
                {
                    // Yeni whitelist entry oluştur
                    var whitelistEntry = new WhitelistedUser
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Description = request.Description ?? "Added to whitelist",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.WhitelistedUsers.Add(whitelistEntry);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"User '{request.Username}' added to whitelist successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Kullanıcıyı whitelist'ten çıkar (sadece admin)
        [HttpDelete("remove/{username}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RemoveFromWhitelist(string username)
        {
            try
            {
                var whitelistEntry = await _context.WhitelistedUsers
                    .FirstOrDefaultAsync(w => w.Username == username && w.IsActive);

                if (whitelistEntry == null)
                    return NotFound("User not found in whitelist");

                // Soft delete - sadece deaktif yap
                whitelistEntry.IsActive = false;
                
                await _context.SaveChangesAsync();

                return Ok(new { message = $"User '{username}' removed from whitelist successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Kendi whitelist durumunu kontrol et
        [HttpGet("check-status")]
        public async Task<IActionResult> CheckWhitelistStatus()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var isWhitelisted = await _context.WhitelistedUsers
                    .AnyAsync(w => w.UserId == userId && w.IsActive);

                return Ok(new { isWhitelisted = isWhitelisted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Whitelist'e eklenebilecek kullanıcıları getir (sadece admin)
        [HttpGet("available-users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAvailableUsers()
        {
            try
            {
                var availableUsers = await _context.Users
                    .Where(u => !_context.WhitelistedUsers
                        .Any(w => w.UserId == u.Id && w.IsActive))
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        Role = u.Role.ToString(),
                        u.CreatedAt
                    })
                    .ToListAsync();

                return Ok(availableUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // Request modeli
    public class AddToWhitelistRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}