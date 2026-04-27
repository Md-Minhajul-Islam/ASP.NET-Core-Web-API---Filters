using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResourceFilterDemo.Filters
{
    // Custom attribute that implements a synchronous resource filter to validate API key presence
    // Can be applied to controllers or actions as an attribute
    public class ApiKeyValidationResourceFilterAttribute : Attribute, IResourceFilter
    {
        // Name of the HTTP header expected to contain teh API Key
        private const string ApiKeyHeaderName = "X-API-KEY";

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var headers = context.HttpContext.Request.Headers;

            if(!headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                var payload = new
                {
                    Status = 401,
                    Message = "Unauthorized: API Key is missing."
                };

                context.Result = new JsonResult(payload)
                {
                    StatusCode = 401
                };
                return;
            }

            // If API key header is present, no action is taken here and the request proceeds
        }


        // This method runs after the action method executes
        // No post-processing is needed in this filter for after the action execution
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // No implementation needed here
        }
    }
}