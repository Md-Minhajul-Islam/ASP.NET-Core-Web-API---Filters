using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFilterDemo.Filters
{
    public class ExecutionTimeLoggingFilter : ActionFilterAttribute
    {
        private Stopwatch? _stopwatch;

        private readonly ILogger<ExecutionTimeLoggingFilter> _logger;

        public ExecutionTimeLoggingFilter(ILogger<ExecutionTimeLoggingFilter> logger)
        {
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _stopwatch = Stopwatch.StartNew();
            var executedContext = await next();
            _stopwatch.Stop();

            var actionName = context.ActionDescriptor.DisplayName;

            _logger.LogInformation($"Execution Time for {actionName} : {_stopwatch.ElapsedMilliseconds} ms");
        }
    }
}