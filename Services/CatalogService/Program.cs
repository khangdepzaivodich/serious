using CatalogService.CatalogServices.Implementations;
using CatalogService.CatalogServices.Interfaces;
using CatalogService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
//options.UseInMemoryDatabase("CatalogSVDb_Memory"); chạy db trên ram ko cần cài sql để test api

builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<ILoaiDanhMucService, LoaiDanhMucService>();
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
builder.Services.AddScoped<IChiTietSanPhamService, ChiTietSanPhamService>();
builder.Services.AddScoped<CatalogService.CatalogServices.IPhotoService, CatalogService.CatalogServices.PhotoService>();
builder.Services.Configure<CatalogService.Helpers.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // Pass the correct file path: content root is the project folder for CatalogService
    var jsonPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "data.json");

    // [HACK FIX] Đảm bảo cột LuotBan tồn tại
    try 
    {
        var conn = dbContext.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        using (var command = conn.CreateCommand())
        {
            command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SanPhams') AND name = 'LuotBan') ALTER TABLE SanPhams ADD LuotBan INT NOT NULL DEFAULT 0;";
            await command.ExecuteNonQueryAsync();
        }
        Console.WriteLine("[CATALOG] Schema check: LuotBan column is ready.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("[CATALOG] Manual schema fix info: " + ex.Message);
    }

    await Seeder.SeedAsync(dbContext, jsonPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
