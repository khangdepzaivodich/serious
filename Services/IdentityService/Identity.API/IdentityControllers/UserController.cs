using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityService.Identity.API.IdentityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    //[Authorize] debug
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly IdentityService.Services.IPhotoService _photoService;

        public UserController(IUserService service, IdentityService.Services.IPhotoService photoService)
        {
            _service = service;
            _photoService = photoService;
        }

        [HttpPost("me/avatar")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var userId = GetUserId();
            var user = await _service.GetMe(userId);
            if (user == null) return NotFound("User not found");

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            // Cập nhật URL ảnh vào User
            // Giả sử UpdateMe có thể cập nhật avatar hoặc ta tạo UpdateAvatar
            // Ở đây ta dùng UpdateMeRequest nếu nó có trường Avatar, nếu không mình sẽ sửa DTO
            var updateRequest = new UpdateMeRequest
            {
                HoTen = user.HoTen,
                SoDienThoai = user.SoDienThoai,
                DiaChi = user.DiaChi,
                NgaySinh = user.NgaySinh,
                GioiTinh = user.GioiTinh,
                Avatar = result.SecureUrl.AbsoluteUri
            };

            await _service.UpdateMe(userId, updateRequest);

            return Ok(new { url = result.SecureUrl.AbsoluteUri });
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var data = await _service.GetMe(GetUserId());
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(UpdateMeRequest request)
        {
            var userId = GetUserId();
            var ok = await _service.UpdateMe(userId, request);
            if (!ok) return NotFound();
            
            var updatedUser = await _service.GetMe(userId);
            return Ok(updatedUser);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var result = await _service.ChangePassword(GetUserId(), request);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var result = await _service.GetAll(page, pageSize);
            return Ok(new { total = result.total, data = result.data });
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _service.GetById(id);
            if (data == null) return NotFound();
            return Ok(data);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var result = await _service.Create(request);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.userId);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateUserByAdminRequest request)
        {
            var ok = await _service.UpdateByAdmin(id, request);
            if (!ok) return NotFound();
            return Ok("Updated");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.Delete(id);
            if (!ok) return NotFound();
            return Ok("Deleted");
        }

        [HttpPut("{id:guid}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Lock(Guid id)
        {
            var ok = await _service.Lock(id);
            if (!ok) return NotFound();
            return Ok("Locked");
        }

        [HttpPut("{id:guid}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unlock(Guid id)
        {
            var ok = await _service.Unlock(id);
            if (!ok) return NotFound();
            return Ok("Unlocked");
        }
        [HttpPost("exists")]
        public async Task<IActionResult> CheckUserExists([FromBody] CheckUserExistsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email is required");

            var exists = await _service.UserExistsByEmail(request.Email);

            return Ok(new CheckUserExistsResponse
            {
                Exists = exists
            });
        }
        [HttpGet("exists/{id}")]
        public async Task<IActionResult> CheckUserExistsById(Guid id)
        {
            var exists = await _service.UserExistsById(id);

            return Ok(new CheckUserExistsResponse
            {
                Exists = exists
            });
        }
    }
}