using Microsoft.AspNetCore.SignalR.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<ChatService.ChatAPI.Services.ChatMongoService>();

builder.Services.AddSingleton<ChatService.ChatAPI.Services.ChatRedisService>();

builder.Services.AddControllers();

// [CORS] Cho phép Frontend Blazor gọi tới
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Cho phép tất cả các cổng localhost
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // SignalR BẮT BUỘC phải có dòng này
    });
});

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

// Middleware CORS
app.UseCors("AllowBlazor");

app.UseAuthorization();

app.MapControllers();

// [SignalR]
app.MapHub<ChatService.ChatAPI.ChatHub>("/chat-hub");

app.Run();
