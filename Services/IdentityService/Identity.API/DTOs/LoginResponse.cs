using System;

namespace IdentityService.Identity.API.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }
}
