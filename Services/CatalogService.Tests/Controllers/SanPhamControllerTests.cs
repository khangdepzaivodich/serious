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
    public class SanPhamControllerTests
    {
        private readonly Mock<ISanPhamService> _mockService;
        private readonly SanPhamController _controller;

        public SanPhamControllerTests()
        {
            _mockService = new Mock<ISanPhamService>();
            _controller = new SanPhamController(_mockService.Object);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenProductExists()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var expectedProduct = new SanPhamDTO { MaSP = productId, TenSP = "Test Product" };
            _mockService.Setup(s => s.GetSanPhamByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProduct = okResult.Value.Should().BeOfType<SanPhamDTO>().Subject;
            returnedProduct.MaSP.Should().Be(productId);
            returnedProduct.TenSP.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockService.Setup(s => s.GetSanPhamByIdAsync(productId)).ReturnsAsync((SanPhamDTO)null);

            // Act
            var result = await _controller.GetById(productId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetBySlug_ShouldReturnOk_WhenProductExists()
        {
            // Arrange
            var slug = "test-product";
            var expectedProduct = new SanPhamDTO { MaSP = Guid.NewGuid(), TenSP = "Test Product", Slug = slug };
            _mockService.Setup(s => s.GetSanPhamBySlugAsync(slug)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetBySlug(slug);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProduct = okResult.Value.Should().BeOfType<SanPhamDTO>().Subject;
            returnedProduct.Slug.Should().Be(slug);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var createDto = new SanPhamCreateDTO { TenSP = "New Product" };
            var createdProduct = new SanPhamDTO { MaSP = Guid.NewGuid(), TenSP = "New Product" };
            _mockService.Setup(s => s.CreateSanPhamAsync(createDto)).ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(SanPhamController.GetById));
            var returnedProduct = createdResult.Value.Should().BeOfType<SanPhamDTO>().Subject;
            returnedProduct.TenSP.Should().Be("New Product");
        }
    }
}
