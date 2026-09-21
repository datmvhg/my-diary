using Microsoft.EntityFrameworkCore;
using DBConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Get connection string from DATABASE_URL environment variable or appsettings.json
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

var connectionString = ConvertPostgresUrlToConnectionString(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Automatically apply pending database migrations on startup
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration notice: {ex.Message}");
}

// Swagger API documentation
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyDiary API v1");
});

app.MapOpenApi();

app.UseCors("VueClient");

// Serve Vue 3 SPA frontend files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to index.html for Vue client-side SPA routing
app.MapFallbackToFile("index.html");

app.Run();

static string ConvertPostgresUrlToConnectionString(string? urlOrConnectionString)
{
    if (string.IsNullOrWhiteSpace(urlOrConnectionString)) return "";
    if (!urlOrConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) && 
        !urlOrConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return urlOrConnectionString;
    }

    try
    {
        var uri = new Uri(urlOrConnectionString);
        var userInfo = uri.UserInfo.Split(':');
        var username = userInfo.Length > 0 ? userInfo[0] : "";
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Prefer;Trust Server Certificate=true;";
    }
    catch
    {
        return urlOrConnectionString;
    }
}