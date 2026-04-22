using IdentityService.Identity.API.Data;
using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using IdentityService.Identity.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Identity.API.IdentityServices.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwt;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwt = jwtOptions.Value;
            Console.WriteLine("JWT Secret" + _jwt.Secret);
        }

        public async Task<(bool Success, string Message)> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                return (false, "Email already exists");

            var user = new User
            {
                MaTK = Guid.NewGuid(),
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                MatKhauHash = HashPassword(request.MatKhau),
                HoTen = request.HoTen,
                DiaChi = request.DiaChi,
                VaiTro = "User",
                TrangThai = "Active",
                NgayThangNamSinh = request.NgayThangNamSinh,
                LastActiveAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Register success");
        }

        public async Task<(bool Success, string Message, object? Data)> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return (false, "Invalid", null);

            if (user.MatKhauHash != HashPassword(request.MatKhau))
                return (false, "Invalid", null);

            var token = GenerateJwt(user);

            var data = new
            {
                token,
                userId = user.MaTK,
                email = user.Email,
                role = user.VaiTro
            };

            return (true, "Login success", data);
        }

        // ===== JWT =====
        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaTK.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro ?? "User")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Secret));

            var creds = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}