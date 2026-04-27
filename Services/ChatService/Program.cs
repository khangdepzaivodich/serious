using Microsoft.AspNetCore.SignalR.StackExchangeRedis;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using ChatService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ChatService.ChatAPI.Services.ChatMongoService>();

builder.Services.AddSingleton<ChatService.ChatAPI.Services.ChatRedisService>();

// Đăng ký Worker
builder.Services.AddHostedService<ChatService.ChatAPI.Services.ChatCleanupWorker>();

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

// [TÍNH NĂNG MỚI] KÍCH HOẠT SignalR WebSockets! ---
var redisConn = builder.Configuration["Redis:ConnectionString"] 
                ?? builder.Configuration.GetConnectionString("Redis") 
                ?? "redis:6379,abortConnect=false";

builder.Services.AddSignalR().AddStackExchangeRedis(redisConn);

// -------------------------------------------------

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

        // Đặc biệt cho SignalR: Đọc token từ Query String
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat-hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

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

// [SignalR]
app.MapHub<ChatService.ChatAPI.ChatHub>("/chat-hub");

app.Run();

// Helper để lấy RSA Public Key cho việc xác thực
static SecurityKey GetIssuerSigningKey(JwtSettings settings)
{
    if (string.IsNullOrEmpty(settings.RsaPublicKey))
    {
        throw new Exception("RSA Public Key is missing in ChatService configuration.");
    }

    var rsa = RSA.Create();
    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(settings.RsaPublicKey), out _);
    return new RsaSecurityKey(rsa);
}

