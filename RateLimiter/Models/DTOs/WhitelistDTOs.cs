namespace RateLimiter.Models.DTOs
{
    public class WhitelistedUserDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public string UserRole { get; set; } = string.Empty;
    }

    public class AddToWhitelistRequest
    {
        public string Username { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class AddToWhitelistResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public WhitelistedUserDto? WhitelistedUser { get; set; }
    }

    public class RemoveFromWhitelistResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
} 