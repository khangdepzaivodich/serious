using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Mã xác nhận (Token) không được để trống")]
        public required string Token { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        public required string NewPassword { get; set; }
    }
}
