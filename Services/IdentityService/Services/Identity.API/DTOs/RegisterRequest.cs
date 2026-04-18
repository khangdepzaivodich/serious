namespace IdentityService.Services.Identity.API.DTOs
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string MatKhau { get; set; }
        public required string SoDienThoai { get; set; }
        public required string HoTen { get; set; }
        public required string DiaChi { get; set; }
        public DateTime NgayThangNamSinh { get; set; }
    }
}
