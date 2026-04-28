using Microsoft.AspNetCore.Mvc;

namespace ExceptionFiltersDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        // HTTP GET method for the route: api/demo/argument
        [HttpGet("argument")]
        public IActionResult ThrowArgumentException()
        {
            // Intentionally throw an ArgumentException with a specific message
            // This simulates a scenario where the user provides invalid input
            // If you have an exception filter registered, it will catch this and return a 400 Bad Request response
            throw new ArgumentException("Invalid parameter value provided.");
        }

        // HTTP GET method for the route: api/demo/notfound
        [HttpGet("notfound")]
        public IActionResult ThrowNotFoundException()
        {
            // Intentionally throw a KeyNotFoundException with a custom message
            // This simulates a scenario where a requested resource does not exist
            // Exception filters can catch this and return a 404 Not Found response with a custom error message
            throw new KeyNotFoundException("The requested item was not found.");
        }

        // HTTP GET method for the route: api/demo/unexpected
        [HttpGet("unexpected")]
        public IActionResult ThrowUnexpectedException()
        {
            // Throw a generic Exception to simulate an unhandled server error
            // Without custom handling, this results in a 500 Internal Server Error response
            // Exception filters can log this and return a friendly error response
            throw new Exception("Simulated unhandled server error.");
        }

        // HTTP GET method for the route: api/demo/ok
        [HttpGet("ok")]
        public IActionResult GetOk()
        {
            // Returns an HTTP 200 OK response with a JSON payload
            // Indicates that the endpoint executed successfully without any exceptions
            return Ok(new { Message = "No exception, everything is OK!" });
        }
    }
}