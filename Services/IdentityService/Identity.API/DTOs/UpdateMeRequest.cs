namespace IdentityService.Identity.API.DTOs
{
    public class UpdateMeRequest
    {
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? Avatar { get; set; }
    }
}
