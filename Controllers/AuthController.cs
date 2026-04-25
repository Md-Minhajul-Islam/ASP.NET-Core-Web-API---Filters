using AuthorizationFilterDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthorizationFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // In-memory hardcoded users list for demo (simulate a user database)
        private readonly List<User> _users = new List<User>()
        {
            new User {Id = 1, Email ="Alice@Example.com", Name = "Alice", Password = "alice123", Roles = "Admin,Manager" },
            new User {Id = 2, Email ="Bob@Example.com", Name = "Bob", Password = "bob123", Roles = "User" },
            new User {Id = 3, Email ="Charlie@Example.com", Name = "Charlie", Password = "charlie123", Roles = "Manager,User" }
        };

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This endpoint is accessible by anyone, even unauthenticated users
        [HttpPost("login")]
        [AllowAnonymous] 
        public IActionResult Login([FromBody] LoginDTO login)
        {
            // Find a user in our hardcoded list that matches the provided email and password (case-insensitive email check)
            var user = _users.FirstOrDefault(u =>
                u.Email.Equals(login.Email, StringComparison.OrdinalIgnoreCase)
                && u.Password == login.Password);

            if (user == null)
            {
                // If no matching user, credentials are invalid — respond with 401 Unauthorized status code
                return Unauthorized("Invalid username or password");
            }

            // Create a list of claims to embed inside the JWT token for this user
            var claims = new List<Claim>
            {
                // Claim to identify the user by their email address
                new Claim(ClaimTypes.Name, user.Email),

                // Custom claim with user's unique Id
                new Claim("UserId", user.Id.ToString()),
            };

            // Add claims for each role assigned to the user (roles are comma-separated string)
            var roles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                // Add a role claim for each role
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }

            // Generate a symmetric security key from the secret configured in appsettings.json
            var secretKey = _configuration.GetValue<string>("JwtSettings:SecretKey") ?? "d3011f8b98bbc1aa1c4ff1a7d4864fc72d9ee150bd682cf4e612d6321f57821d";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Specify signing credentials using HMAC SHA256 algorithm and the generated key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create a JWT token embedding the claims, with no issuer/audience for simplicity, and expiration set to 30 minutes from now
            var token = new JwtSecurityToken(
                issuer: null, // No specific issuer specified.
                audience: null, // No specific audience specified.
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Token valid for 30 minutes
                signingCredentials: creds);

            // Serialize the JWT token to a string
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Return the JWT token string as JSON to the client
            return Ok(new { Token = tokenString });
        }
    }
}