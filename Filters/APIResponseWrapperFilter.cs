using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionFilterDemo.Filters
{
    public class APIResponseWrapperFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
           if(context.Result is ObjectResult objectResult)
            {
                var wrappedResponse = new
                {
                    Status = objectResult.StatusCode ?? 200,
                    Message = "Success",
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
                    Data = null as object
                };

                context.Result = new JsonResult(wrappedResponse)
                {
                    StatusCode = 204
                };
            }
            // Call the base method to ensure any additional processing is performed
            base.OnActionExecuted(context);

        }
    }
}