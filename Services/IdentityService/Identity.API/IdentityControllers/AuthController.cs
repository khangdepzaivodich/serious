using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Identity.API.IdentityControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.Register(request);

            return StatusCode(result.Success ? 200 : 400, new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authService.Login(request);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPassword(request);

            return StatusCode(result.Success ? 200 : 400, new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);

            return StatusCode(result.Success ? 200 : 400, new
            {
                success = result.Success,
                message = result.Message
            });
        }

        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                UserId = User.FindFirst("sub")?.Value,
                Email = User.FindFirst("email")?.Value,
                Name = User.FindFirst("name")?.Value,
                Role = User.FindFirst("role")?.Value
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult CheckAdmin()
        {           
            return Ok(new { success = true, message = "User is Admin" });
        }
    }
}