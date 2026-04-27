using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using BasketService.BasketAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using BasketService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddOpenApi();

// =====================
// Authentication (RSA)
// =====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);

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

            IssuerSigningKey = GetIssuerSigningKey(jwtSettings),
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// Cấu hình Redis: Ưu tiên lấy từ biến môi trường Redis__ConnectionString (Docker Compose)
// Đọc trực tiếp từ env var để tránh .NET config parse sai ký tự đặc biệt (dấu phẩy, dấu =)
var redisConnectionString = Environment.GetEnvironmentVariable("Redis__ConnectionString")
                            ?? builder.Configuration["Redis:ConnectionString"]
                            ?? builder.Configuration.GetConnectionString("Redis") 
                            ?? "localhost:6379,abortConnect=false";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddScoped<IBasketService, BasketRedisService>();

// Cấu hình Authentication Delegation
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthenticationDelegatingHandler>();

// Lấy Cấu hình hệ thống HTTP Client
builder.Services.AddHttpClient<ICatalogService, CatalogApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"] ?? "http://localhost:5289");
});

builder.Services.AddHttpClient<IIdentityService, IdentityApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:IdentityUrl"] ?? "http://localhost:5018");
}).AddHttpMessageHandler<AuthenticationDelegatingHandler>(); // Tự động chèn Bearer Token của User vào IdentityService

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Helper để lấy RSA Public Key cho việc xác thực
static SecurityKey GetIssuerSigningKey(JwtSettings settings)
{
    if (string.IsNullOrEmpty(settings.RsaPublicKey))
    {
        throw new Exception("RSA Public Key is missing in BasketService configuration.");
    }

    var rsa = RSA.Create();
    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(settings.RsaPublicKey), out _);
    return new RsaSecurityKey(rsa);
}

