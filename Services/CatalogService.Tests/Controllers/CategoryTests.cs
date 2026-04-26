using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogService.CatalogControllers;
using CatalogService.CatalogServices.Interfaces;
using CatalogService.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;

namespace CatalogService.Tests.Controllers
{
    public class DanhMucControllerTests
    {
        private readonly Mock<IDanhMucService> _mockService;
        private readonly DanhMucController _controller;

        public DanhMucControllerTests()
        {
            _mockService = new Mock<IDanhMucService>();
            _controller = new DanhMucController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var categories = new List<DanhMucDTO> { new DanhMucDTO { MaDM = Guid.NewGuid(), TenDM = "Cate 1", MaLDM = Guid.NewGuid() } };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedData = okResult.Value.Should().BeAssignableTo<IEnumerable<DanhMucDTO>>().Subject;
            returnedData.Should().HaveCount(1);
        }

        [Fact]
        public async Task Get_ShouldReturnNotFound_WhenIdInvalid()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((DanhMucDTO)null);

            // Act
            var result = await _controller.Get(Guid.NewGuid());

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }

    public class LoaiDanhMucControllerTests
    {
        private readonly Mock<ILoaiDanhMucService> _mockService;
        private readonly LoaiDanhMucController _controller;

        public LoaiDanhMucControllerTests()
        {
            _mockService = new Mock<ILoaiDanhMucService>();
            _controller = new LoaiDanhMucController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk()
        {
            // Arrange
            var types = new List<LoaiDanhMucDTO> { new LoaiDanhMucDTO { MaLDM = Guid.NewGuid(), TenLDM = "Type 1" } };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(types);

            // Act
            var result = await _controller.GetAll();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
