using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFilterDemo.Filters
{
    public class ComplexInputValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Try to get the "StartDate" argument from the action method parameters
            // and assign it to 'startDateObj' if it exists
            // Similarly, try to get "EndDate" argument and assign to 'endDateObj'
            if(context.ActionArguments.TryGetValue("StartDate", out var startDateObj)
                && context.ActionArguments.TryGetValue("EndDate", out var endDateObj)
                // Check if both objects are of type DateTime, and cast them accordingly
                && startDateObj is DateTime startDate && endDateObj is DateTime endDate)
            {
                if(startDate > endDate)
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        Status = 400,
                        Messae = "StarteDate cannot be later than EndDate."
                    });
                    return;
                }
            }
            await next();
        }
    }
}