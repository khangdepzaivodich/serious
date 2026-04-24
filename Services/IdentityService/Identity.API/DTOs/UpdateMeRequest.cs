using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class UpdateMeRequest
    {
        [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
        public string? HoTen { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string? DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? Avatar { get; set; }
    }
}
