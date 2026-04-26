using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityService.Identity.API.DTOs;
using IdentityService.Identity.API.IdentityControllers;
using IdentityService.Identity.API.IdentityServices.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace IdentityService.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IdentityService.Services.IPhotoService> _mockPhotoService;
        private readonly UserController _controller;
        private readonly Guid _testUserId = Guid.NewGuid();

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockPhotoService = new Mock<IdentityService.Services.IPhotoService>();
            _controller = new UserController(_mockUserService.Object, _mockPhotoService.Object);

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims }
            };
        }

        [Fact]
        public async Task Me_ShouldReturnOkResult_WithUserData_WhenUserExists()
        {
            // Arrange
            var expectedUser = new UserDto 
            { 
                MaTK = _testUserId, 
                Email = "test@example.com", 
                HoTen = "Khang Nguyen" 
            };
            _mockUserService.Setup(s => s.GetMe(_testUserId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _controller.Me();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            returnedUser.MaTK.Should().Be(_testUserId);
        }

        [Fact]
        public async Task Me_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetMe(_testUserId)).ReturnsAsync((UserDto)null);

            // Act
            var result = await _controller.Me();

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateMe_ShouldReturnOkResult_WithUpdatedUser_WhenSuccessful()
        {
            // Arrange
            var request = new UpdateMeRequest { HoTen = "Updated Name" };
            var updatedUser = new UserDto { MaTK = _testUserId, HoTen = "Updated Name" };
            
            _mockUserService.Setup(s => s.UpdateMe(_testUserId, request)).ReturnsAsync(true);
            _mockUserService.Setup(s => s.GetMe(_testUserId)).ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateMe(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedUser = okResult.Value.Should().BeAssignableTo<UserDto>().Subject;
            returnedUser.HoTen.Should().Be("Updated Name");
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var request = new ChangePasswordRequest { OldPassword = "old", NewPassword = "new" };
            _mockUserService.Setup(s => s.ChangePassword(_testUserId, request))
                            .ReturnsAsync((true, "Password changed"));

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Password changed");
        }

        [Fact]
        public async Task CheckUserExists_ShouldReturnTrue_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            var request = new CheckUserExistsRequest { Email = email };
            _mockUserService.Setup(s => s.UserExistsByEmail(email)).ReturnsAsync(true);

            // Act
            var result = await _controller.CheckUserExists(request);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<CheckUserExistsResponse>().Subject;
            response.Exists.Should().BeTrue();
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenAdminDeletesUser()
        {
            // Arrange
            var targetId = Guid.NewGuid();
            _mockUserService.Setup(s => s.Delete(targetId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(targetId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Deleted");
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithData_WhenAdminCalls()
        {
            // Arrange
            var users = new List<UserDto> { new UserDto { MaTK = Guid.NewGuid(), Email = "admin@test.com" } };
            _mockUserService.Setup(s => s.GetAll(1, 20)).ReturnsAsync((1, users));

            // Act
            var result = await _controller.GetAll(1, 20);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnBadRequest_WhenServiceReturnsFailure()
        {
            // Arrange
            var request = new ChangePasswordRequest { OldPassword = "wrong", NewPassword = "new" };
            _mockUserService.Setup(s => s.ChangePassword(_testUserId, request))
                            .ReturnsAsync((false, "Mật khẩu cũ không đúng"));

            // Act
            var result = await _controller.ChangePassword(request);

            // Assert
            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.Value.Should().Be("Mật khẩu cũ không đúng");
        }

        [Fact]
        public async Task Lock_ShouldReturnOk_WhenAdminLocksUser()
        {
            // Arrange
            var targetId = Guid.NewGuid();
            _mockUserService.Setup(s => s.Lock(targetId)).ReturnsAsync(true);

            // Act
            var result = await _controller.Lock(targetId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be("Locked");
        }
    }
}
