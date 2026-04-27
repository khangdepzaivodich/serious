using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using DiscountService.Discount.API.Data;
using DiscountService.Discount.API.DiscountServices.Implementations;
using DiscountService.Discount.API.DiscountServices.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using DiscountService;

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

// =====================
// Authentication (RSA)
// =====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,

            IssuerSigningKey = GetIssuerSigningKey(jwtSettings),
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Helper để lấy RSA Public Key cho việc xác thực
static SecurityKey GetIssuerSigningKey(JwtSettings settings)
{
    if (string.IsNullOrEmpty(settings.RsaPublicKey))
    {
        throw new Exception("RSA Public Key is missing in DiscountService configuration.");
    }

    var rsa = RSA.Create();
    rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(settings.RsaPublicKey), out _);
    return new RsaSecurityKey(rsa);
}
