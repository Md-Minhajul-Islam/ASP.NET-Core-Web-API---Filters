using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CustomAuthFilterDemo.Filters
{
    // Custom asynchronous authorization filter that restricts access based on user's department claim
    public class DeprtmentAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly string _allowedDepartment;

        public DeprtmentAuthorizationFilter(string allowedDepartment)
        {
            _allowedDepartment = allowedDepartment;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            bool isAuthenticated = user?.Identity?.IsAuthenticated == true;

            if (!isAuthenticated)
            {
                // User is not authenticated - respond with 401 Unauthorized JSON error
                context.Result = CreateJsonResponse(
                    401,                             // HTTP status code
                    "Unauthorized",                  // Error title
                    "Authentication is required to access this resource."  // Message
                );
                return; // Stop further pipeline execution
            }
            var department = user?.FindFirst("Department")?.Value;
            // Check if department claim is missing or does NOT match the allowed department (case-insensitive)
            if (department == null || !string.Equals(department, _allowedDepartment, StringComparison.OrdinalIgnoreCase))
            {
                // Return 403 Forbidden with a JSON error indicating department restriction
                context.Result = CreateJsonResponse(
                    403,
                    "Forbidden",
                    $"Access restricted to {_allowedDepartment} department only."
                );
                return; // Stop further pipeline execution
            }
            // Since this method is async, complete the task to satisfy interface contract
            await Task.CompletedTask;
        }
        // Helper method to create a JSON response with status, error, and message fields
        private JsonResult CreateJsonResponse(int statusCode, string error, string message)
        {
            // Create anonymous object to represent JSON payload
            var jsonPayload = new
            {
                Status = statusCode,  // HTTP status code (401 or 403)
                Error = error,        // Error type string
                Message = message     // Human-readable message
            };
            // Return the JSON response with appropriate HTTP status code
            return new JsonResult(jsonPayload)
            {
                StatusCode = statusCode
            };
        }
    }
}