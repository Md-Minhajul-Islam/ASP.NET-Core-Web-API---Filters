using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthorizationFilterDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Read the JWT secret key from the appsettings.json configuration file.
            // This key will be used to sign and validate JWT tokens.
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "d3011f8b98bbc1aa1c4ff1a7d4864fc72d9ee150bd682cf4e612d6321f57821d";

            // Register MVC controllers with the application.
            // Also, configure JSON options to keep property names as defined in the C# models.
            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                // Disable camelCase in JSON output, preserve property names as defined in C# classes
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Register Authentication with JWT Bearer scheme
            builder.Services.AddAuthentication(options =>
            {
                // These two options set JWT Bearer as the default scheme for authentication and challenge.
                // This means the middleware will look for JWT tokens in incoming requests by default.

                // Set the default scheme used for authentication — this means how the app will try to authenticate incoming requests
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configure parameters for validating incoming JWT tokens
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Do NOT validate the issuer (the token's "iss" claim)
                    ValidateIssuer = false,

                    // Do NOT validate the audience (the token's "aud" claim)
                    ValidateAudience = false,

                    // Ensure the token's signature matches the signing key (to verify token integrity)
                    ValidateIssuerSigningKey = true,

                    // The key used to sign tokens — must match the key used to generate tokens
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // Use a symmetric key from configuration for token validation.
                };
            });

            //Define a policy
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminAndManager", policy =>
                    policy.RequireRole("Admin")    // must have Admin
                          .RequireRole("Manager")  // AND must also have Manager
                );
            });

            // builder.Services.AddEndpointsApiExplorer();
            // builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // if (app.Environment.IsDevelopment())
            // {
            //     app.UseSwagger();
            //     app.UseSwaggerUI();
            // }

            app.UseHttpsRedirection();

            // Add authentication middleware to validate JWT tokens in incoming requests.
            app.UseAuthentication();  // MUST come before UseAuthorization

            // Add authorization middleware to check user permissions for accessing resources.
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}