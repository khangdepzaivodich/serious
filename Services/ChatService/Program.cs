using Microsoft.AspNetCore.SignalR.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ChatService.ChatAPI.ChatMongoService>();

builder.Services.AddSingleton<ChatService.ChatAPI.ChatRedisService>();

builder.Services.AddControllers();

// [TÍNH NĂNG MỚI] KÍCH HOẠT SignalR WebSockets! ---
builder.Services.AddSignalR().AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis")!);

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

app.UseAuthorization();

app.MapControllers();

// [SignalR] Cắm cái đuôi cho trạm phát sóng (Đây là link mà Blazor Frontend sẽ gõ vào để connect WebSocket)
app.MapHub<ChatService.ChatAPI.ChatHub>("/chat-hub");

app.Run();
