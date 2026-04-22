using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityServices.Interfaces;
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
    }
}