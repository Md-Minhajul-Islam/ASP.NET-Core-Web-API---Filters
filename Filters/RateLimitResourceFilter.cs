using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ResourceFilterDemo.Filters
{
    // This class impmements an asynchronous resource filter for rate limiting API requests by client IP address
    public class RateLimitResourceFilter : IAsyncResourceFilter
    {
        // Injected memory cache instance used to track request count per client IP
        private readonly IMemoryCache _cache;
        // Maximum allowed request per minute per client IP
        private readonly int _maxRequestsPerMinute = 5;

        public RateLimitResourceFilter(IMemoryCache cache)
        {
            _cache = cache;
        }

        // This async method runs before the action method executes
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            // Get the client's IP address as a string; fallback to "unknown" if not found
            var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            // Compose a unique cache key per IP to track request count
            var cachekey = $"RateLimit_{ip}";
            // Try to get current request count for this IP from cache; default to 0 if not present
            var requestCount = _cache.Get<int?>(cachekey) ?? 0;
            // Check if the client has already reached or exceeded the request limit
            if(requestCount >= _maxRequestsPerMinute)
            {
                var errorResponse = new
                {
                    Status = 429,
                    Message = "Too many requests. Please try again later."
                };

                context.Result = new JsonResult(errorResponse)
                {
                    StatusCode = 429
                };
                return;
            }
            else
            {
                // Increment the request count and store it back in cache with a 1-minute expiration
                _cache.Set(cachekey, requestCount+1, TimeSpan.FromMinutes(1));
            }
            // If rate limit not exceeded proceed with the request execution pipeline
            await next();
            // After action executes

        }   
    }
}