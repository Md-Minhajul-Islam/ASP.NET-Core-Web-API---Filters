using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CustomAuthFilterDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CustomAuthFilterDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuratoin;

        public AuthController(IConfiguration configuration)
        {
            _configuratoin = configuration;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var user = UserStore.Users.FirstOrDefault(u => 
            u.Email.Equals(login.Email, StringComparison.OrdinalIgnoreCase)
            && u.Password == login.Password);

            if(user == null)
            {
                return  Unauthorized("Invalid username or password");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("SubscriptionLevel", user.SubscriptionLevel ?? "Free"),
                new Claim("Department", user.Department ?? "None")
            };

            if(user.SubscriptionExpiresOn != null)
            {
                claims.Add(new Claim("SubscriptionExpiresOn", user.SubscriptionExpiresOn.Value.ToString()));
            }

            var secretKey = _configuratoin.GetValue<string>("JwtSettings:SecretKey");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );
        
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new {Token = tokenString});        
        }
    }
}