using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ResultFiltersDemo.Filters
{
    // Custom Result Filter for auditing/logging details about each response sent to clients
    public class AuditLoggingResultFilter : ResultFilterAttribute
    {
        private readonly ILogger<AuditLoggingResultFilter> _logger;

        // Constructor with dependency injection of ILogger for logging capabilities
        public AuditLoggingResultFilter(ILogger<AuditLoggingResultFilter> logger)
        {
            _logger = logger;
        }

        // This method runs after the action result has been executed and the response is ready
        public override void OnResultExecuted(ResultExecutedContext context)
        {
            // Get the current HTTP context (contains request and response info)
            var httpContext = context.HttpContext;

            // Get the HTTP response object for status code, headers, etc.
            var response = httpContext.Response;

            // Get the authenticated user principal associated with the request
            var user = httpContext.User;

            // Attempt to extract the user's ID from their claims (NameIdentifier claim)
            // If not found (unauthenticated), fallback to "Anonymous"
            string userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            // Get the approximate size of the response payload in bytes if available
            long? payloadSize = response.ContentLength;

            // Log the audit details: status code, user ID, payload size, request path, and timestamp (UTC)
            _logger.LogInformation("Audit Log - Response Sent: " +
                                   $"StatusCode={response.StatusCode}, UserId={userId}, PayloadSize={payloadSize} bytes, " +
                                   $"Path={httpContext.Request.Path}, Timestamp={DateTime.UtcNow}");

            // Call the base class implementation (if any additional behavior is defined)
            base.OnResultExecuted(context);
        }
    }
}