using ResourceFilterDemo.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register Memory Cache
builder.Services.AddMemoryCache();

// Register the Resource Filter
// No need to Register the ResourceFilter Attributes
builder.Services.AddScoped<WeatherCacheResourceFilter>();
builder.Services.AddScoped<RateLimitResourceFilter>();

var app = builder.Build();



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
