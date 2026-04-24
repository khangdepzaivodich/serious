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
        private readonly IEmailService _emailService;
        private readonly string _frontendUrl;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _jwt = jwtOptions.Value;
            _emailService = emailService;
            _frontendUrl = configuration["FrontendUrl"] ?? "";
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
                SoDienThoai = request.SoDienThoai ?? "",
                MatKhauHash = HashPassword(request.MatKhau),
                HoTen = request.HoTen,
                DiaChi = request.DiaChi ?? "",
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
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return (false, "Invalid", null);

            if (user.MatKhauHash != HashPassword(request.MatKhau))
                return (false, "Invalid", null);

            var token = GenerateJwt(user);

            var data = new LoginResponse
            {
                Token = token,
                UserId = user.MaTK,
                Email = user.Email,
                Role = user.VaiTro ?? "User",
                HoTen = user.HoTen,
                Avatar = string.IsNullOrWhiteSpace(user.Avatar) ? null : user.Avatar
            };

            return (true, "Login success", data);
        }

        // ===== Forgot / Reset Password =====
        public async Task<(bool Success, string Message)> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return (false, "User not found");

            var token = GenerateSecureToken();
            user.ResetToken = token;
            user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(30);

            await _context.SaveChangesAsync();

            // build reset link
            var resetLink = $"{_frontendUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={token}";

            var subject = "Password reset instructions";
            var htmlBody = $@"
                <p>Hello {user.HoTen ?? user.Email},</p>
                <p>You requested a password reset. Click the link below to set a new password. The link expires in 30 minutes.</p>
                <p><a href=""{resetLink}"">Reset password</a></p>
                <p>If you did not request this, ignore this email.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to send email: {ex.Message}");
            }

            return (true, "Password reset email sent");
        }

        public async Task<(bool Success, string Message)> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return (false, "User not found");

            if (string.IsNullOrEmpty(user.ResetToken) || user.ResetToken != request.Token)
                return (false, "Invalid token");

            if (!user.ResetTokenExpires.HasValue || user.ResetTokenExpires.Value < DateTime.UtcNow)
                return (false, "Token expired");

            user.MatKhauHash = HashPassword(request.NewPassword);

            // Clear token
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return (true, "Password reset successful");
        }

        // ===== JWT =====
        private string GenerateJwt(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaTK.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen ?? ""),
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

        private static string GenerateSecureToken(int size = 32)
        {
            var bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            // Base64Url safe
            var token = Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return token;
        }
    }
}