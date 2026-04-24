using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public required string MatKhau { get; set; }
    }
}
