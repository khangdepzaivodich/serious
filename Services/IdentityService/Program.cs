using IdentityService.Identity.API;
using IdentityService.Identity.API.Data;
using IdentityService.Identity.API.IdentityServices.Implementations;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// =====================
// Controllers
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:7119", "http://localhost:5270")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
builder.Services.AddControllers();

// =====================
// DbContext
// =====================
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// =====================
// JWT Settings
// =====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// =====================
// SMTP Settings & Email service
// =====================
var smtpSection = builder.Configuration.GetSection("SmtpSettings");
builder.Services.Configure<SmtpSettings>(smtpSection);
builder.Services.AddSingleton<IEmailService, EmailService>();
// Note: EmailService takes IOptions<SmtpSettings> in its constructor

// =====================
// DI Services
// =====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IdentityService.Services.IPhotoService, IdentityService.Services.PhotoService>();
builder.Services.Configure<IdentityService.Helpers.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// =====================
// Swagger
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================
// Authentication
// =====================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)
            ),

            RoleClaimType = ClaimTypes.Role
        };
    });

// =====================
// Authorization
// =====================
builder.Services.AddAuthorization();

// =====================
// Build
// =====================
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    await IdentitySeeder.SeedDefaultAdminAsync(dbContext);
}

// =====================
// Middleware
// =====================
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].ToString();
    if (string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine($"[AUTH LOG] Missing Authorization Header for {context.Request.Path}");
    }
    else 
    {
        Console.WriteLine($"[AUTH LOG] Found Header: {authHeader.Substring(0, Math.Min(authHeader.Length, 30))}...");
    }
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
