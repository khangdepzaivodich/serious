using backend.Services.Identity.API.DTOs;
using backend.Services.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Identity.API.IdentityControllers
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

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
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