using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResultFiltersDemo.Filters
{
    public class AddCustomHeadersResultFilter : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {

            var response = context.HttpContext.Response;

            
            // Add a custom header "X-API-Version" with value "1.0" if it is not already present
            if (!response.Headers.ContainsKey("X-API-Version"))
            {
                response.Headers["X-API-Version"] = "1.0";
            }
            

            // Conditionally add Cache-Control header only for:
            // 1. HTTP GET requests (safe to cache)
            // 2. Responses where the result is an ObjectResult (typical JSON result)
            // 3. The response status code is 200 OK (success)
            if(context.HttpContext.Request.Method == "GET"
                && context.Result is ObjectResult objectResult
                && (objectResult.StatusCode ?? 200) == 200)
            {
                response.Headers["Cache-Control"] = "public, max-age=3600";
            }


            base.OnResultExecuting(context);
        }
    }
}