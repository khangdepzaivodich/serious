using IdentityService.Identity.API;
using IdentityService.Identity.API.Data;
using IdentityService.Identity.API.IdentityServices.Implementations;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// =====================
// Controllers
// =====================
builder.Services.AddControllers();
// =====================
// CORS
// =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// =====================
// DbContext
// =====================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    //options.UseInMemoryDatabase("IdentitySVDb_Memory")); // chạy db trên ram ko cần cài sql để test api

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    //dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =====================
// JWT Settings (OPTIONS pattern)
// =====================
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// =====================
// DI Services
// =====================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

// =====================
// Swagger
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =====================
// Build temporary config for JWT validation
// =====================
var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret))
{
    throw new Exception("JwtSettings is missing or invalid");
}

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

var app = builder.Build();

// =====================
// Middleware
// =====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();