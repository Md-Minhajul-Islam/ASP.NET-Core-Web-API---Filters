namespace ResultFiltersDemo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers()
           .AddJsonOptions(options =>
            {
               // Disable camelCase in JSON output, preserve property names as defined in C# classes
               options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}