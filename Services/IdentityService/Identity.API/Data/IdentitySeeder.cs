using System.Security.Cryptography;
using System.Text;
using IdentityService.Identity.API.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Identity.API.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedDefaultAdminAsync(AppDbContext context)
        {
            const string adminEmail = "admin@gmail.com";
            const string adminPassword = "123456";

            var admin = await context.Users.FirstOrDefaultAsync(x => x.Email == adminEmail);
            var adminPasswordHash = HashPassword(adminPassword);

            if (admin == null)
            {
                admin = new User
                {
                    MaTK = Guid.NewGuid(),
                    Email = adminEmail,
                    SoDienThoai = "0900000000",
                    MatKhauHash = adminPasswordHash,
                    HoTen = "Administrator",
                    DiaChi = "System",
                    VaiTro = "Admin",
                    TrangThai = "Active",
                    NgayThangNamSinh = new DateTime(2000, 1, 1),
                    LastActiveAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
            }
            else
            {
                admin.MatKhauHash = adminPasswordHash;
                admin.VaiTro = "Admin";
                admin.TrangThai = "Active";
                admin.LastActiveAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
