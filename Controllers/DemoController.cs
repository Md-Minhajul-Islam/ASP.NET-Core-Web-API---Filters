using CustomAuthFilterDemo.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CustomAuthFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DemoController : ControllerBase
    {
        [HttpGet("premium-analytics")]
        [TypeFilter(typeof(SubscriptionBasedAuthorizatoinFilter), Arguments = new object[] {new [] { "Premium", "Pro"}})]
        public IActionResult GetPremiumAnalytics()
        {
            return Ok(new {message = "Welcome to Premium Analytics"});
        }

        [HttpGet("salary-review")]
        [TypeFilter(typeof(DeprtmentAuthorizationFilter), Arguments = new object[] {"HR"})]
        public IActionResult GetSalaryReview()
        {
            return Ok(new {message = "HR department salary review data"});
        }

        [HttpGet("SUPPORT-Ticket")]
        [BusinessHoursAuthorize(9, 18)]
        public IActionResult GetSupportTicket()
        {
            return Ok(new {message = "Support ticket API (business hours only) accessed."});
        }
    }
}