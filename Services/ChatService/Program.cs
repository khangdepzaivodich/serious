using Microsoft.AspNetCore.SignalR.StackExchangeRedis;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

// [SignalR]
app.MapHub<ChatService.ChatAPI.ChatHub>("/chat-hub");

app.Run();

