using Microsoft.AspNetCore.Mvc;
using ResultFiltersDemo.Filters;

namespace ResultFiltersDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        // Action method to demonstrate Global Response Wrapping filter usage
        // Route: GET api/demo/wrapped-response
        // This applies the ApiResponseWrapperResultFilter to wrap responses in a standard format
        [HttpGet("wrapped-response")]
        [TypeFilter(typeof(ApiResponseWrapperResultFilter))]
        public IActionResult GetWrappedResponse()
        {
            // Prepare sample data to return
            var data = new { Id = 1, Name = "John Doe" };

            // Return HTTP 200 OK with the data; the filter will wrap this response
            return Ok(data);
        }

        // Action method to demonstrate adding/modifying HTTP response headers
        // Route: GET api/demo/headers
        // This applies AddCustomHeadersResultFilter to add headers like X-API-Version and Cache-Control
        [HttpGet("headers")]
        [TypeFilter(typeof(AddCustomHeadersResultFilter))]
        public IActionResult GetWithCustomHeaders()
        {
            // Prepare sample message to return
            var data = new { Message = "Headers should be modified in the response." };

            // Return HTTP 200 OK with the message; the filter adds custom headers to this response
            return Ok(data);
        }

        // Action method to simulate a large response to test compression filter
        // Route: GET api/demo/large-response
        // Applies ResponseCompressionResultFilter to compress JSON responses larger than 100 KB
        [HttpGet("large-response")]
        [TypeFilter(typeof(ResponseCompressionResultFilter))]
        public IActionResult GetLargeResponse()
        {
            // Generate a large string of 150 KB size (150 * 1024 characters)
            // 1 char = 2 byte
            // 512 char = 1 Kb
            // 1024 char = 2 Kb
            // 150 * 1024 char = 300 kb
            var largeData = new string('A', 150 * 1024);

            // Return the large data wrapped in an object; filter will compress this response
            return Ok(new { Content = largeData });
        }

        // Action method to demonstrate audit logging of responses
        // Route: GET api/demo/audit-logging
        // Applies AuditLoggingResultFilter to log response details after execution
        [HttpGet("audit-logging")]
        [TypeFilter(typeof(AuditLoggingResultFilter))]
        public IActionResult GetForAuditLogging()
        {
            // Prepare a list of sample entries
            var info = new List<string> { "Entry1", "Entry2", "Entry3" };

            // Return HTTP 200 OK with the list; filter logs this response's details
            return Ok(info);
        }

        // Action method demonstrating combining multiple filters on a single action
        // Route: GET api/demo/combined
        // Applies response wrapping, header modification, and audit logging filters together
        [HttpGet("combined")]
        [TypeFilter(typeof(ApiResponseWrapperResultFilter))]
        [TypeFilter(typeof(AddCustomHeadersResultFilter))]
        [TypeFilter(typeof(AuditLoggingResultFilter))]
        public IActionResult GetCombined()
        {
            // Prepare sample status data
            var data = new { Status = "Multiple filters applied" };

            // Return HTTP 200 OK with data; all specified filters run in order on this response
            return Ok(data);
        }
    }
}