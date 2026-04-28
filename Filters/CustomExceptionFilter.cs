using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ExceptionFiltersDemo.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        private readonly IHostEnvironment _env;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger, IHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occured.");

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var error = "Internal Server Error";
            var message = "An unexpected error occurred. Please try again later.";

            if (_env.IsDevelopment())
            {
                message = context.Exception.Message;
            }

            switch (context.Exception)
            {
                // Handle argument validation errors (e.g., invalid method arguments)
                case ArgumentException argEx:
                    statusCode = (int) HttpStatusCode.BadRequest;
                    error = "Bad Request";
                    message = argEx.Message;
                    break;
                
                // Handle resource not found errors
                case KeyNotFoundException _:
                    statusCode = (int)HttpStatusCode.NotFound;
                    error = "Not Found";
                    message = "Resource not found.";
                    break;

                // Handle authentication failures
                case AuthenticationException _:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    error = "Unauthorized";
                    message = "Authentication failed.";
                    break;

                // Handle unauthorized access attempts
                case UnauthorizedAccessException _:
                    statusCode = (int)HttpStatusCode.Forbidden;
                    error = "Forbiddent";
                    message = "You do not have permission to access this resource.";
                    break;

                // Handle invalid operations (e.g., state conflicts)
                case InvalidOperationException _:
                    statusCode = (int)HttpStatusCode.Conflict; 
                    error = "Conflict";
                    message = context.Exception.Message;         
                    break;

                // Handle SQL Server exceptions (e.g., connection failures)
                case SqlException _:
                // Handle Entity Framework update exceptions (e.g., database update issues)
                case DbUpdateException _:
                    statusCode = (int)HttpStatusCode.ServiceUnavailable;
                    error = "Database Error";
                    message = "A database error occurred. Please try again later.";
                    break;

                // Handle unimplemented functionality exceptions
                case NotImplementedException _:
                    statusCode = (int)HttpStatusCode.NotImplemented; 
                    error = "Not Implemented";
                    message = "This functionality is not implemented.";
                    break;

                // Handle request timeouts
                case TimeoutException _:
                    statusCode = (int)HttpStatusCode.RequestTimeout;
                    error = "Request Timeout";
                    message = "The request timed out. Please try again.";
                    break;
            }

            var errorResponse = new
            {
                Status = statusCode,
                Error = error,
                Message = message,
                From = "Exception Filter"
            };

            context.Result = new JsonResult(errorResponse)
            {
                StatusCode = statusCode
            };

            context.ExceptionHandled = true;
        }

    }
}