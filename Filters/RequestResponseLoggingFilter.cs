using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFilterDemo.Filters
{
    public class RequestResponseLoggingFilter : IActionFilter
    {
        private readonly ILogger<RequestResponseLoggingFilter> _logger;
        public RequestResponseLoggingFilter(ILogger<RequestResponseLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var request = httpContext.Request;
            var query = JsonSerializer.Serialize(request.Query);
            var routeData = JsonSerializer.Serialize(context.RouteData.Values);
            var headers = JsonSerializer.Serialize(request.Headers);

            _logger.LogInformation($"Request Incoming: Method={request.Method}, Path={request.Path}, Query={query}, RouteData={routeData}, Headers={headers}");

        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;

            _logger.LogInformation($"Response Outgoing: StatusCode={response.StatusCode}");
        }
    }
}