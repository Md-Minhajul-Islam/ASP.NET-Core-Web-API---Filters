using ActionFilterDemo.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ActionFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        [HttpGet("log-request-response")]
        [TypeFilter(typeof(RequestResponseLoggingFilter))]
        public IActionResult LogRequestResponseDemo([FromQuery] string sampleParam)
        {
            return Ok(new { Message = "Request and response logged successfully.", Param = sampleParam });
        }

        [HttpGet("validate-input")]
        [TypeFilter(typeof(ComplexInputValidationFilter))]
        public IActionResult ValidateInputDemo([FromQuery] DateTime StartDate, [FromQuery] DateTime EndDate)
        {
            return Ok(new { Message = "Input validated successfully.", StartDate, EndDate });
        }

        [HttpGet("standardized-response")]
        [APIResponseWrapperFilter]
        public IActionResult StadardizeResponseDemo()
        {
            var data = new { Value = 123, Description = "Some data" };
            return Ok(data);
        }
        
        [HttpGet("execution-time")]
        [TypeFilter(typeof(ExecutionTimeLoggingFilter))]
        public async Task<IActionResult> ExecutionTimeDemo()
        {
            // Simulate processing delay of 500 milliseconds to mimic some workload
            await Task.Delay(500);
            return Ok(new { Message = "Execution time logged." });
        }


    }

   
    
}