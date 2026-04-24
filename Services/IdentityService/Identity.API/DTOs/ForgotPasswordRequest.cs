using System.ComponentModel.DataAnnotations;

namespace IdentityService.Identity.API.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public required string Email { get; set; }
    }
}