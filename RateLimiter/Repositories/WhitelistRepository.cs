using Microsoft.EntityFrameworkCore;
using RateLimiter.Data;
using RateLimiter.Interfaces;
using RateLimiter.Models;

namespace RateLimiter.Repositories
{
    public class WhitelistRepository : IWhitelistRepository
    {
        private readonly AppDbContext _context;

        public WhitelistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WhitelistedUser>> GetAllWhitelistedUsersAsync()
        {
            return await _context.WhitelistedUsers
                .Include(w => w.User)
                .Where(w => w.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<WhitelistedUser?> GetWhitelistedUserByUserIdAsync(int userId)
        {
            return await _context.WhitelistedUsers
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<WhitelistedUser?> GetWhitelistedUserByUsernameAsync(string username)
        {
            return await _context.WhitelistedUsers
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Username == username && w.IsActive);
        }

        public async Task<bool> AddToWhitelistAsync(WhitelistedUser whitelistedUser)
        {
            try
            {
                _context.WhitelistedUsers.Add(whitelistedUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateWhitelistedUserAsync(WhitelistedUser whitelistedUser)
        {
            try
            {
                _context.WhitelistedUsers.Update(whitelistedUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromWhitelistAsync(string username)
        {
            try
            {
                var whitelistEntry = await _context.WhitelistedUsers
                    .FirstOrDefaultAsync(w => w.Username == username && w.IsActive);

                if (whitelistEntry == null) return false;

                // Soft delete - sadece deaktif yap
                whitelistEntry.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserWhitelistedAsync(int userId)
        {
            return await _context.WhitelistedUsers
                .AnyAsync(w => w.UserId == userId && w.IsActive);
        }
    }
} 