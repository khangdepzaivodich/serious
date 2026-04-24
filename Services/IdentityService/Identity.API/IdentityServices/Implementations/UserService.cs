using IdentityService.Identity.API.Data;
using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using IdentityService.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Identity.API.IdentityServices.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<UserDto?> GetMe(Guid userId)
        {
            return await _context.Users
                .Where(x => x.MaTK == userId)
                .Select(x => new UserDto
                {
                    MaTK = x.MaTK,
                    Email = x.Email,
                    HoTen = x.HoTen,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    VaiTro = x.VaiTro,
                    TrangThai = x.TrangThai,
                    NgaySinh = x.NgayThangNamSinh,
                    Avatar = x.Avatar,
                    GioiTinh = x.GioiTinh,
                    LastActiveAt = x.LastActiveAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateMe(Guid userId, UpdateMeRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == userId);
            if (user == null) return false;

            user.HoTen = request.HoTen ?? user.HoTen;
            user.SoDienThoai = request.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = request.DiaChi ?? user.DiaChi;
            user.NgayThangNamSinh = request.NgaySinh ?? user.NgayThangNamSinh;
            user.Avatar = request.Avatar ?? user.Avatar;
            user.GioiTinh = request.GioiTinh ?? user.GioiTinh;
            user.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> ChangePassword(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == userId);
            if (user == null) return (false, "Not found");

            if (user.MatKhauHash != HashPassword(request.OldPassword))
                return (false, "Old password wrong");

            user.MatKhauHash = HashPassword(request.NewPassword);
            user.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Password changed");
        }

        public async Task<(int total, IEnumerable<UserDto> data)> GetAll(int page, int pageSize)
        {
            var query = _context.Users.AsNoTracking();

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.LastActiveAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserDto
                {
                    MaTK = x.MaTK,
                    Email = x.Email,
                    HoTen = x.HoTen,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    VaiTro = x.VaiTro,
                    TrangThai = x.TrangThai,
                    NgaySinh = x.NgayThangNamSinh,
                    Avatar = x.Avatar,
                    GioiTinh = x.GioiTinh,
                    LastActiveAt = x.LastActiveAt
                })
                .ToListAsync();

            return (total, data);
        }

        public async Task<UserDto?> GetById(Guid id)
        {
            return await _context.Users
                .Where(x => x.MaTK == id)
                .Select(x => new UserDto
                {
                    MaTK = x.MaTK,
                    Email = x.Email,
                    HoTen = x.HoTen,
                    SoDienThoai = x.SoDienThoai,
                    DiaChi = x.DiaChi,
                    VaiTro = x.VaiTro,
                    TrangThai = x.TrangThai,
                    NgaySinh = x.NgayThangNamSinh,
                    Avatar = x.Avatar,
                    GioiTinh = x.GioiTinh,
                    LastActiveAt = x.LastActiveAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool Success, string Message, Guid? userId)> Create(CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                return (false, "Email exists", null);

            var user = new User
            {
                MaTK = Guid.NewGuid(),
                Email = request.Email,
                SoDienThoai = request.SoDienThoai ?? "",
                MatKhauHash = HashPassword(request.Password),
                HoTen = request.HoTen,
                DiaChi = request.DiaChi ?? "",
                VaiTro = request.VaiTro ?? "User",
                TrangThai = "Active",
                NgayThangNamSinh = request.NgayThangNamSinh,
                LastActiveAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return (true, "Created", user.MaTK);
        }

        public async Task<bool> UpdateByAdmin(Guid id, UpdateUserByAdminRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return false;

            user.HoTen = request.HoTen ?? user.HoTen;
            user.SoDienThoai = request.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = request.DiaChi ?? user.DiaChi;
            user.VaiTro = request.VaiTro ?? user.VaiTro;
            user.TrangThai = request.TrangThai ?? user.TrangThai;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Lock(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return false;

            user.TrangThai = "Blocked";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Unlock(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return false;

            user.TrangThai = "Active";
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UserExistsByEmail(string email)
        {
            return await _context.Users
                .AnyAsync(x => x.Email == email);
        }
        public async Task<bool> UserExistsById(Guid userId)
        {
            return await _context.Users
                .AnyAsync(x => x.MaTK == userId);
        }
    }
}