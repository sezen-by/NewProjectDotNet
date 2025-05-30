using RateLimiter.DTOs;
using RateLimiter.Interfaces;
using RateLimiter.Models;
using RateLimiter.Services.Interfaces;

namespace RateLimiter.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(request.Username);

                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı!"
                    };
                }

                if (!VerifyPassword(request.Password, user.Password))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Hatalı şifre!"
                    };
                }

                var token = _jwtService.GenerateToken(user);

                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    Role = user.Role.ToString(),
                    Message = "Giriş başarılı!"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Login hatası: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var userExists = await _userRepository.UserExistsAsync(request.Username);

                if (userExists)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Bu kullanıcı adı zaten kullanılıyor!"
                    };
                }

                var hashedPassword = HashPassword(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    Password = hashedPassword,
                    Role = UserRole.user, // Default role
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _userRepository.CreateUserAsync(user);

                if (!created)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Kullanıcı oluşturulamadı!"
                    };
                }

                return new AuthResponse
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla kayıt edildi!"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = $"Kayıt hatası: {ex.Message}"
                };
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
} 