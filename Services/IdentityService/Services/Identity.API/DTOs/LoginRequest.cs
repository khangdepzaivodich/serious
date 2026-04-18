namespace IdentityService.Services.Identity.API.DTOs
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string MatKhau { get; set; }
    }
}
