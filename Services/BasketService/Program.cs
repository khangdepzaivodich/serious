using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using BasketService.BasketAPI.Services;

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

// Cấu hình Redis: Ưu tiên lấy từ biến môi trường Redis:ConnectionString (Docker Compose)
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] 
                            ?? builder.Configuration.GetConnectionString("Redis") 
                            ?? "redis:6379,abortConnect=false";

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
app.UseAuthorization();
app.MapControllers();

app.Run();

