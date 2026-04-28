using ExceptionFiltersDemo.Filters;
using ExceptionFiltersDemo.Middlewares;
namespace ExceptionFiltersDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers(options =>
            {
                // Register the Exception Filter Globally
                // Remove this for Middleware Exception Handler
                options.Filters.Add<CustomExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
               // Disable camelCase in JSON output, preserve property names as defined in C# classes
               options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });


            var app = builder.Build();


            app.UseHttpsRedirection();

            
            //app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}