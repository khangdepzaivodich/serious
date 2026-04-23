using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using DiscountService.Discount.API.Data;
using DiscountService.Discount.API.DiscountServices.Implementations;
using DiscountService.Discount.API.DiscountServices.Interfaces;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));

// Add CORS

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
app.UseCors("AllowAll");


app.UseAuthorization();
app.MapControllers();

app.Run();
