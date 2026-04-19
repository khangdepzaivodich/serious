using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderingService.Ordering.API.DTOs;
using OrderingService.Ordering.API.OrderingServices.Interfaces;

namespace OrderingService.Ordering.API.OrderingControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonHangController : ControllerBase
    {
        private readonly IDonHangService _donHangService;

        public DonHangController(IDonHangService donHangService)
        {
            _donHangService = donHangService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDonHang([FromBody] CreateDonHangRequest request)
        {
            var result = await _donHangService.CreateDonHangAsync(request);
            return CreatedAtAction(nameof(GetDonHangById), new { maDH = result.MaDH }, result);
        }

        [HttpGet("{maDH}")]
        public async Task<IActionResult> GetDonHangById(Guid maDH)
        {
            var donHang = await _donHangService.GetDonHangByIdAsync(maDH);
            if (donHang == null) return NotFound();
            return Ok(donHang);
        }

        [HttpGet("user/{maTK}")]
        public async Task<IActionResult> GetDonHangsByUserId(Guid maTK)
        {
            var donHangs = await _donHangService.GetDonHangsByUserIdAsync(maTK);
            return Ok(donHangs);
        }

        [HttpPatch("{maDH}/status")]
        public async Task<IActionResult> UpdateDonHangStatus(Guid maDH, [FromBody] string newStatus)
        {
            var success = await _donHangService.UpdateDonHangStatusAsync(maDH, newStatus);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}