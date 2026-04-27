using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace ResourceFilterDemo.Filters
{
    // This class implements a resource filter to cache weather data responses per city
    public class WeatherCacheResourceFilter : IResourceFilter
    {
        // Injected memory cache instance to store and retrieve cached data
        private readonly IMemoryCache _memoryCache;
        // Defines how long cached data will be stored (60 seconds here)
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

        public WeatherCacheResourceFilter(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            // Extract the 'city' route parameter from the request URL, convert to lowercase for consistency
            var city = context.RouteData.Values["city"]?.ToString()?.ToLower();
            // If city is missing or empty, do nothing and let the request proces
            
            if(string.IsNullOrEmpty(city)) return;
            // If you are using query string for city
            // var city = context.HttpContext.Request.Query["city"].ToString()?.ToLower();
            // Attempt to retrieve cached data for this city from the memory cache
            // 'cachedData' will contain the cached weather data if it exists
            if(_memoryCache.TryGetValue(city, out object? cachedData) && cachedData != null)
            {
                // If cached data found, immediately short-circuit the request pipeline
                // by returning the cached data as JSON with HTTP status 200 OK
                context.Result = new JsonResult(cachedData)
                {
                    StatusCode = 200    
                };
            }
            // If no cached data found, the request proceeds to the action method normally
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // Extract the 'city' route parameter again for caching the response
            var city = context.RouteData.Values["city"]?.ToString()?.ToLower();

            // If you are using query string for city
            // var city = context.HttpContext.Request.Query["city"].ToString()?.ToLower();
            // If city is missing or empty, do nothing (cannot cache without key)
            if (string.IsNullOrEmpty(city)) 
                return;
            
            // Check if the action method returned a successful 200 OK result with non-null value
            if(context.Result is ObjectResult result && result.StatusCode == 200 && result.Value is not null)
            {
                // Store the result value in the memory cache with the city as the key
                // Cache duration is set to CacheDuration (60 seconds)
                _memoryCache.Set(city, result.Value, CacheDuration);
            }
            // If the result is not succesful or value is null, do not cache anything
        }

    }
}