using backend.Services.Identity.API.Data;
using backend.Services.Identity.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Services.Identity.API.DTOs;
namespace backend.Services.Identity.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // tất cả endpoint cần login
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ===== Helpers =====
        private Guid GetUserId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(id!);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ================= SELF =================

        // GET /api/user/me
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = GetUserId();

            var user = await _context.Users
                .Where(x => x.MaTK == userId)
                .Select(x => new
                {
                    x.MaTK,
                    x.Email,
                    x.HoTen,
                    x.SoDienThoai,
                    x.DiaChi,
                    x.VaiTro,
                    x.TrangThai,
                    x.LastActiveAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT /api/user/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UpdateMeRequest request)
        {
            var userId = GetUserId();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == userId);
            if (user == null) return NotFound();

            user.HoTen = request.HoTen ?? user.HoTen;
            user.SoDienThoai = request.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = request.DiaChi ?? user.DiaChi;
            user.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Updated");
        }

        // PUT /api/user/change-password
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = GetUserId();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == userId);
            if (user == null) return NotFound();

            if (user.MatKhauHash != HashPassword(request.OldPassword))
                return BadRequest("Old password wrong");

            user.MatKhauHash = HashPassword(request.NewPassword);
            user.LastActiveAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok("Password changed");
        }

        // ================= ADMIN =================

        // GET /api/user (list)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.Users.AsNoTracking();

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.LastActiveAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.MaTK,
                    x.Email,
                    x.HoTen,
                    x.SoDienThoai,
                    x.VaiTro,
                    x.TrangThai,
                    x.LastActiveAt
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        // GET /api/user/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _context.Users
                .Where(x => x.MaTK == id)
                .Select(x => new
                {
                    x.MaTK,
                    x.Email,
                    x.HoTen,
                    x.SoDienThoai,
                    x.DiaChi,
                    x.VaiTro,
                    x.TrangThai,
                    x.NgayThangNamSinh,
                    x.LastActiveAt
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST /api/user (create by admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                return BadRequest("Email exists");

            var user = new User
            {
                MaTK = Guid.NewGuid(),
                Email = request.Email,
                SoDienThoai = request.SoDienThoai,
                MatKhauHash = HashPassword(request.Password),
                HoTen = request.HoTen,
                DiaChi = request.DiaChi,
                VaiTro = request.VaiTro ?? "User",
                TrangThai = "Active",
                NgayThangNamSinh = request.NgayThangNamSinh,
                LastActiveAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user.MaTK);
        }

        // PUT /api/user/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateByAdmin(Guid id, UpdateUserByAdminRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return NotFound();

            user.HoTen = request.HoTen ?? user.HoTen;
            user.SoDienThoai = request.SoDienThoai ?? user.SoDienThoai;
            user.DiaChi = request.DiaChi ?? user.DiaChi;
            user.VaiTro = request.VaiTro ?? user.VaiTro;
            user.TrangThai = request.TrangThai ?? user.TrangThai;

            await _context.SaveChangesAsync();
            return Ok("Updated");
        }

        // DELETE /api/user/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }

        // PUT /api/user/{id}/lock
        [HttpPut("{id:guid}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lock(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return NotFound();

            user.TrangThai = "Blocked";
            await _context.SaveChangesAsync();

            return Ok("Locked");
        }

        // PUT /api/user/{id}/unlock
        [HttpPut("{id:guid}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unlock(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.MaTK == id);
            if (user == null) return NotFound();

            user.TrangThai = "Active";
            await _context.SaveChangesAsync();

            return Ok("Unlocked");
        }
    }


    




}