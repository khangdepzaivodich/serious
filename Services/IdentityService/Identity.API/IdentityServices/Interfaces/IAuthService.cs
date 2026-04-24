using IdentityService.Identity.API.DTOs;

namespace IdentityService.Identity.API.IdentityServices.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> Register(RegisterRequest request);
        Task<(bool Success, string Message, object? Data)> Login(LoginRequest request);

        // Forgot/reset password
        Task<(bool Success, string Message)> ForgotPassword(ForgotPasswordRequest request);
        Task<(bool Success, string Message)> ResetPassword(ResetPasswordRequest request);
    }
}
