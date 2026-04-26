using System.Threading.Tasks;
using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityControllers;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace IdentityService.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", MatKhau = "123" };
            var loginResponse = new LoginResponse { Token = "mock-jwt-token" };
            
            _mockAuthService.Setup(s => s.Login(request))
                            .ReturnsAsync((true, "Success", loginResponse));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var data = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
            data.Token.Should().Be("mock-jwt-token");
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginRequest { Email = "wrong@test.com", MatKhau = "wrong" };
            
            _mockAuthService.Setup(s => s.Login(request))
                            .ReturnsAsync((false, "Invalid email or password", null));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorized.Value.Should().Be("Invalid email or password");
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new RegisterRequest 
            { 
                Email = "new@test.com", 
                MatKhau = "123456",
                HoTen = "Test User",
                SoDienThoai = "0123456789"
            };
            _mockAuthService.Setup(s => s.Register(request))
                            .ReturnsAsync((true, "User registered successfully"));

            // Act
            var result = await _controller.Register(request);

            // Assert
            var resultObject = result.Should().BeOfType<ObjectResult>().Subject;
            resultObject.StatusCode.Should().Be(200);
            resultObject.Value.ToString().Should().Contain("True");
        }
    }
}
