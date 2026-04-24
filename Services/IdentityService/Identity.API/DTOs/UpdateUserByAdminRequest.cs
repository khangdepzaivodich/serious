using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class UpdateUserByAdminRequest
    {
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string? HoTen { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string? DiaChi { get; set; }

        public string? VaiTro { get; set; }
        public string? TrangThai { get; set; }
    }
}
