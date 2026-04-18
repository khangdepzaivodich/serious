using backend.Services.Identity.API.DTOs;
namespace backend.Services.Identity.API.IdentityServices.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> Register(RegisterRequest request);
        Task<(bool Success, string Message, object? Data)> Login(LoginRequest request);
    }
}
