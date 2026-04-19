using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using BasketService.BasketAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Cấu hình Redis từ appsettings
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379,ssl=False,abortConnect=False";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// Đăng ký BasketService
builder.Services.AddScoped<IBasketService, BasketRedisService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
