namespace RateLimiter.Configuration
{
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting:Default";
 
        public int MaxRequests { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
    }
} 