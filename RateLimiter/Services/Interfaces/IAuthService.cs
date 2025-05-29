using Microsoft.AspNetCore.Mvc;
using RateLimiter.Models;
using RateLimiter.DTOs;

namespace RateLimiter.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
    }
} 