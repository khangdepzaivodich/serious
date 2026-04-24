using System;

namespace IdentityService.Identity.API.DTOs
{
    public class UserDto
    {
        public Guid MaTK { get; set; }
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public string VaiTro { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }
        public string? Avatar { get; set; }
        public string? GioiTinh { get; set; }
        public DateTime? LastActiveAt { get; set; }
    }
}
