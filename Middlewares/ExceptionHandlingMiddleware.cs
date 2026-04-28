using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace ExceptionFiltersDemo.Middlewares
{
    // Custom middleware to handle exceptions globally
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;          // Next middleware in the pipeline
        private readonly ILogger<ExceptionHandlingMiddleware> _logger; // Logger for error logging
        private readonly IHostEnvironment _env;         // To determine environment (Dev/Prod)

        // Constructor with DI for next delegate, logger, and environment
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        // This method is called for each HTTP request
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Call the next middleware/component in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                // Catch any exception thrown downstream
                await HandleExceptionAsync(context, ex);
            }
        }

        // Centralized method to handle exceptions and write custom response
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log exception details with stack trace
            _logger.LogError(exception, "Unhandled exception caught by middleware.");

            // Default to 500 Internal Server Error
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var error = "Internal Server Error";
            var message = "An unexpected error occurred. Please try again later.";

            // Customize message for development environment
            if (_env.IsDevelopment())
            {
                message = exception.Message;
            }

            // Use a switch statement to handle specific exception types differently
            switch (exception)
            {
                // Handle argument validation errors (e.g., invalid method arguments)
                case ArgumentException argEx:
                    statusCode = (int)HttpStatusCode.BadRequest;  // 400 Bad Request
                    error = "Bad Request";
                    message = argEx.Message;                       // Use exception's message
                    break;

                // Handle resource not found errors
                case KeyNotFoundException _:
                    statusCode = (int)HttpStatusCode.NotFound;    // 404 Not Found
                    error = "Not Found";
                    message = "Resource not found.";               // Generic message for client
                    break;

                // Handle authentication failures
                case AuthenticationException _:
                    statusCode = (int)HttpStatusCode.Unauthorized; // 401 Unauthorized
                    error = "Unauthorized";
                    message = "Authentication failed.";
                    break;

                // Handle unauthorized access attempts
                case UnauthorizedAccessException _:
                    statusCode = (int)HttpStatusCode.Unauthorized; // 401 Unauthorized
                    error = "Unauthorized";
                    message = "Unauthorized access.";
                    break;

                // Handle invalid operations (e.g., state conflicts)
                case InvalidOperationException _:
                    statusCode = (int)HttpStatusCode.Conflict;     // 409 Conflict
                    error = "Conflict";
                    message = exception.Message;           // Use detailed message
                    break;

                // Handle SQL Server exceptions (e.g., connection failures)
                case SqlException _:
                // Handle Entity Framework update exceptions (e.g., database update issues)
                case DbUpdateException _:
                    statusCode = (int)HttpStatusCode.ServiceUnavailable; // 503 Service Unavailable
                    error = "Database Error";
                    message = "A database error occurred. Please try again later.";
                    break;

                // Handle unimplemented functionality exceptions
                case NotImplementedException _:
                    statusCode = (int)HttpStatusCode.NotImplemented;    // 501 Not Implemented
                    error = "Not Implemented";
                    message = "This functionality is not implemented.";
                    break;

                // Handle request timeouts
                case TimeoutException _:
                    statusCode = (int)HttpStatusCode.RequestTimeout;    // 408 Request Timeout
                    error = "Request Timeout";
                    message = "The request timed out. Please try again.";
                    break;

                    // You can add more cases here to handle other exception types as needed
            }

            // Prepare error response payload
            var errorResponse = new
            {
                Status = statusCode,
                Error = error,
                Message = message,
                From = "Middleware"
            };

            // Serialize to JSON
            var jsonResponse = JsonSerializer.Serialize(errorResponse);

            // Set response content type and status code
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            // Write JSON error response to the client
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}