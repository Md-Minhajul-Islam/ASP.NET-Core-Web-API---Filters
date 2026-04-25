using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AuthorizationFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // Public endpoint - no authentication required
        [HttpGet("public")]
        [AllowAnonymous] // Allows anyone (even unauthenticated users) to access this endpoint
        public IActionResult Public()
        {
            return Ok("This is a public endpoint accessible to everyone.");
        }

        // Authenticated users only - no role restriction
        [HttpGet("authenticated")]
        [Authorize] // Requires a valid JWT token (any authenticated user)
        public IActionResult Authenticated()
        {
            return Ok($"Hello {User.Identity?.Name}, you are authenticated.");
        }

        // Single Role Authorization - Admin only
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")] // Only users with "Admin" role can access this endpoint
        public IActionResult AdminOnly()
        {
            return Ok("This endpoint is restricted to Admin role users only.");
        }

        // Multiple Roles - AND Logic (User must have both Manager and Admin)
        [HttpGet("manager-and-admin")]
        [Authorize(Roles = "Manager")] // User must have "Manager" role
        [Authorize(Roles = "Admin")] // User must ALSO have "Admin" role
        public IActionResult ManagerAndAdmin()
        {
            return Ok("You must have BOTH Manager AND Admin roles to access this endpoint.");
        }

        // Multiple Roles - OR Logic (User must have Manager OR User)
        [HttpGet("manager-or-user")]
        [Authorize(Roles = "Manager,User")] // User needs at least one of these roles
        public IActionResult ManagerOrUser()
        {
            return Ok("You have either Manager OR User role - access granted.");
        }
    }
}