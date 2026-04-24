using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public required string Password { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public required string HoTen { get; set; }

        public string? DiaChi { get; set; }
        public string? VaiTro { get; set; }
        public DateTime NgayThangNamSinh { get; set; }
    }
}
