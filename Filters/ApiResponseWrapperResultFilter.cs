using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ResultFiltersDemo.Filters
{
    public class ApiResponseWrapperResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if(context.Result is ObjectResult objectResult)
            {
                var wrappedResponse = new
                {
                    Status = objectResult.StatusCode ?? 200,
                    Message = objectResult.StatusCode == 200 ? "Success" : "Error",
                    Data = objectResult.Value
                };

                context.Result = new JsonResult(wrappedResponse)
                {
                    StatusCode = objectResult.StatusCode
                };
            }
            else if(context.Result is EmptyResult)
            {
                var wrappedResponse = new
                {
                    Status = 204,
                    Message = "No Content",
                    Data = (object?) null
                };

                context.Result = new JsonResult(wrappedResponse)
                {
                    StatusCode = 204
                };
            }
        }

        // This method runs after the result has been executed (after response is sent)
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // This is optional for post-result processing and currently left empty
        }
    }
}