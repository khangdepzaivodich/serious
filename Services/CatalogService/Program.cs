using CatalogService.CatalogServices.Interfaces;
using CatalogService.CatalogServices.Implementations;
using CatalogService.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string not found");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
    //options.UseInMemoryDatabase("CatalogSVDb_Memory")); chạy db trên ram ko cần cài sql để test api

builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<ILoaiDanhMucService, LoaiDanhMucService>();
builder.Services.AddScoped<IDanhMucService, DanhMucService>();
builder.Services.AddScoped<IChiTietSanPhamService, ChiTietSanPhamService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
    //dbContext.Database.EnsureCreated();
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