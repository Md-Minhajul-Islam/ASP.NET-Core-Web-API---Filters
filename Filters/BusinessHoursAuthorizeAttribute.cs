using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAuthFilterDemo.Filters
{
    // Custom authorization attribute to restrict API access based on business hours
    // Inherits from AuthorizeAttribute and implements IAuthorizationFilter for synchronous authorization logic
    public class BusinessHoursAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly int _startHour;
        private readonly int _endHour;

        public BusinessHoursAuthorizeAttribute(int startHour = 9, int endHour = 18)
        {
            _startHour = startHour;
            _endHour = endHour;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            bool IsAuthenticated = user?.Identity?.IsAuthenticated == true;
            if (!IsAuthenticated)
            {
                context.Result = CreateJsonResponse(
                    401,                                   
                    "Unauthorized",                       
                    "Authentication is required to access this resource." 
                );
                return; // Stop further pipeline execution
            }
            // Get the current local time of day
            var now = DateTime.Now.TimeOfDay;
            // Check if current hour is outside the allowed business hours range
            if (now.Hours < _startHour || now.Hours >= _endHour)
            {
                // Return 403 Forbidden with JSON error indicating access restriction by time
                context.Result = CreateJsonResponse(
                    403,
                    "Forbidden",
                    $"API accessible only between {_startHour}:00 and {_endHour}:00 local time."
                );
                return; // Stop further pipeline execution
            }
            // If user is authenticated and current time is within business hours, request proceeds normally
        }

        // Helper method to generate consistent JSON error responses with given status code, error, and message
        private JsonResult CreateJsonResponse(int statusCode, string error, string message)
        {
            // Create anonymous object to hold the response payload
            var jsonPayload = new
            {
                Status = statusCode, // HTTP status code e.g. 401 or 403
                Error = error,       // Error string like "Unauthorized" or "Forbidden"
                Message = message    // Human-readable explanation for client
            };
            // Return a JsonResult with the payload and specified HTTP status code
            return new JsonResult(jsonPayload)
            {
                StatusCode = statusCode
            };
        }
    }
}