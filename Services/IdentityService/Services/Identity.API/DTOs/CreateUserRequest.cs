namespace IdentityService.Services.Identity.API.DTOs
{
    public class CreateUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? SoDienThoai { get; set; }
        public required string HoTen { get; set; }
        public string? DiaChi { get; set; }
        public string? VaiTro { get; set; }
        public DateTime NgayThangNamSinh { get; set; }
    }
}
