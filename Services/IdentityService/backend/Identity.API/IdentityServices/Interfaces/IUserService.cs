using backend.Services.Identity.API.DTOs;

namespace backend.Services.Identity.API.IdentityServices.Interfaces
{
    public interface IUserService
    {
        Task<object?> GetMe(Guid userId);
        Task<bool> UpdateMe(Guid userId, UpdateMeRequest request);
        Task<(bool Success, string Message)> ChangePassword(Guid userId, ChangePasswordRequest request);

        Task<(int total, object data)> GetAll(int page, int pageSize);
        Task<object?> GetById(Guid id);
        Task<(bool Success, string Message, Guid? userId)> Create(CreateUserRequest request);
        Task<bool> UpdateByAdmin(Guid id, UpdateUserByAdminRequest request);
        Task<bool> Delete(Guid id);
        Task<bool> Lock(Guid id);
        Task<bool> Unlock(Guid id);
    }
}
