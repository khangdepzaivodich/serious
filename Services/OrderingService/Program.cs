using Microsoft.EntityFrameworkCore;
using OrderingService.Ordering.API.Data;
using OrderingService.Ordering.API.OrderingServices.Implementations;
using OrderingService.Ordering.API.OrderingServices.Interfaces;

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
builder.Services.AddHttpClient("CatalogService", client =>
{
    client.BaseAddress = new Uri("https://catalog-service:8080/"); // URL of Catalog API in Docker
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddHttpClient("DiscountService", client =>
{
    client.BaseAddress = new Uri("https://discount-service:8080/"); // URL of Discount API in Docker
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IDonHangService, DonHangService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // 1. Chạy Migration chuẩn
    dbContext.Database.Migrate();

    // 2. [HACK FIX] Đảm bảo các cột missing được thêm vào nếu Migration bị lag
    try 
    {
        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        
        using (var command = conn.CreateCommand())
        {
            // Kiểm tra và thêm HoTen
            command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DonHangs') AND name = 'HoTen') ALTER TABLE DonHangs ADD HoTen nvarchar(150) NOT NULL DEFAULT '';";
            await command.ExecuteNonQueryAsync();

            // Kiểm tra và thêm SoDienThoai
            command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DonHangs') AND name = 'SoDienThoai') ALTER TABLE DonHangs ADD SoDienThoai nvarchar(20) NOT NULL DEFAULT '';";
            await command.ExecuteNonQueryAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Manual schema fix failed (might already exist): " + ex.Message);
    }
}

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


