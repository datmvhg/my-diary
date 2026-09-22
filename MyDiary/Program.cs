using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DBConnect;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// Register Email Service (MailKit)
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Database Connection
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

var connectionString = ConvertPostgresUrlToConnectionString(rawConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ASP.NET Core Identity Configuration
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MyDiarySuperSecretSecureKeyWithAtLeast32CharactersLong2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyDiaryApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MyDiaryClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
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

// Swagger UI
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

// Authentication & Authorization pipeline
app.UseAuthentication();
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