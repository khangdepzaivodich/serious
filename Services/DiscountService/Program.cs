using DiscountService.Discount.API.Data;
using DiscountService.Discount.API.DiscountServices.Implementations;
using DiscountService.Discount.API.DiscountServices.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure MongoDB Database settings
builder.Services.Configure<DiscountDbSettings>(
    builder.Configuration.GetSection("DiscountDatabase"));

// Register Discount Service
builder.Services.AddSingleton<IMaGiamGiaService, MaGiamGiaService>();

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
