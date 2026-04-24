using IdentityService.Identity.API.DTOs;

namespace IdentityService.Identity.API.IdentityServices.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetMe(Guid userId);
        Task<bool> UpdateMe(Guid userId, UpdateMeRequest request);
        Task<(bool Success, string Message)> ChangePassword(Guid userId, ChangePasswordRequest request);

        Task<(int total, IEnumerable<UserDto> data)> GetAll(int page, int pageSize);
        Task<UserDto?> GetById(Guid id);
        Task<(bool Success, string Message, Guid? userId)> Create(CreateUserRequest request);
        Task<bool> UpdateByAdmin(Guid id, UpdateUserByAdminRequest request);
        Task<bool> Delete(Guid id);
        Task<bool> Lock(Guid id);
        Task<bool> Unlock(Guid id);
        Task<bool> UserExistsByEmail(string email);
        Task<bool> UserExistsById(Guid userId);
    }
}
