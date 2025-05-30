using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Models.DTOs;
using RateLimiter.Services.Interfaces;

namespace RateLimiter.Services
{
    public class WhitelistService : IWhitelistService
    {
        private readonly IWhitelistRepository _whitelistRepository;
        private readonly IUserRepository _userRepository;

        public WhitelistService(IWhitelistRepository whitelistRepository, IUserRepository userRepository)
        {
            _whitelistRepository = whitelistRepository;
            _userRepository = userRepository;
        }

        public async Task<List<WhitelistedUserDto>> GetAllWhitelistedUsersAsync()
        {
            try
            {
                var whitelistedUsers = await _whitelistRepository.GetAllWhitelistedUsersAsync();

                return whitelistedUsers.Select(w => new WhitelistedUserDto
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    Username = w.Username,
                    Description = w.Description,
                    CreatedAt = w.CreatedAt,
                    IsActive = w.IsActive,
                    UserRole = w.User?.Role.ToString() ?? "Unknown"
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting whitelisted users: {ex.Message}");
            }
        }

        public async Task<AddToWhitelistResponse> AddToWhitelistAsync(AddToWhitelistRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(request.Username);
                if (user == null)
                {
                    return new AddToWhitelistResponse
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı!"
                    };
                }

                var existingWhitelistEntry = await _whitelistRepository.GetWhitelistedUserByUserIdAsync(user.Id);

                if (existingWhitelistEntry != null)
                {
                    if (existingWhitelistEntry.IsActive)
                    {
                        return new AddToWhitelistResponse
                        {
                            Success = false,
                            Message = "Kullanıcı zaten whitelist'te!"
                        };
                    }

                    existingWhitelistEntry.IsActive = true;
                    existingWhitelistEntry.Description = request.Description ?? existingWhitelistEntry.Description;

                    var updated = await _whitelistRepository.UpdateWhitelistedUserAsync(existingWhitelistEntry);
                    if (!updated)
                    {
                        return new AddToWhitelistResponse
                        {
                            Success = false,
                            Message = "Whitelist güncellenemedi!"
                        };
                    }

                    return new AddToWhitelistResponse
                    {
                        Success = true,
                        Message = $"Kullanıcı '{request.Username}' başarıyla whitelist'e eklendi!",
                        WhitelistedUser = new WhitelistedUserDto
                        {
                            Id = existingWhitelistEntry.Id,
                            UserId = existingWhitelistEntry.UserId,
                            Username = existingWhitelistEntry.Username,
                            Description = existingWhitelistEntry.Description,
                            CreatedAt = existingWhitelistEntry.CreatedAt,
                            IsActive = existingWhitelistEntry.IsActive,
                            UserRole = user.Role.ToString()
                        }
                    };
                }
                else
                {
                    var whitelistEntry = new WhitelistedUser
                    {
                        UserId = user.Id,
                        Username = user.Username,
                        Description = request.Description ?? "Whitelist'e eklendi",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    var created = await _whitelistRepository.AddToWhitelistAsync(whitelistEntry);
                    if (!created)
                    {
                        return new AddToWhitelistResponse
                        {
                            Success = false,
                            Message = "Whitelist oluşturulamadı!"
                        };
                    }

                    return new AddToWhitelistResponse
                    {
                        Success = true,
                        Message = $"Kullanıcı '{request.Username}' başarıyla whitelist'e eklendi!",
                        WhitelistedUser = new WhitelistedUserDto
                        {
                            Id = whitelistEntry.Id,
                            UserId = whitelistEntry.UserId,
                            Username = whitelistEntry.Username,
                            Description = whitelistEntry.Description,
                            CreatedAt = whitelistEntry.CreatedAt,
                            IsActive = whitelistEntry.IsActive,
                            UserRole = user.Role.ToString()
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new AddToWhitelistResponse
                {
                    Success = false,
                    Message = $"Whitelist ekleme hatası: {ex.Message}"
                };
            }
        }

        public async Task<RemoveFromWhitelistResponse> RemoveFromWhitelistAsync(string username)
        {
            try
            {
                var removed = await _whitelistRepository.RemoveFromWhitelistAsync(username);

                if (!removed)
                {
                    return new RemoveFromWhitelistResponse
                    {
                        Success = false,
                        Message = "Kullanıcı whitelist'te bulunamadı!"
                    };
                }

                return new RemoveFromWhitelistResponse
                {
                    Success = true,
                    Message = $"Kullanıcı '{username}' başarıyla whitelist'ten çıkarıldı!"
                };
            }
            catch (Exception ex)
            {
                return new RemoveFromWhitelistResponse
                {
                    Success = false,
                    Message = $"Whitelist çıkarma hatası: {ex.Message}"
                };
            }
        }
    }
} 