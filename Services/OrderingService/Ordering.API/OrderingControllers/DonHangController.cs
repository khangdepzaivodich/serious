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
            try 
            {
                if (request == null)
                    return BadRequest("Invalid request.");

                if (request.MaTK == Guid.Empty)
                    return BadRequest("User is required.");

                if (request.ChiTietDonHangs == null || request.ChiTietDonHangs.Count == 0)
                    return BadRequest("Cart is empty.");

                if (string.IsNullOrWhiteSpace(request.HoTen) || string.IsNullOrWhiteSpace(request.SoDienThoai) || string.IsNullOrWhiteSpace(request.DiaChiGiaoHang))
                    return BadRequest("Customer information is required.");

                var result = await _donHangService.CreateDonHangAsync(request);
                return CreatedAtAction(nameof(GetDonHangById), new { maDH = result.MaDH }, result);
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine("--- ERROR CREATING ORDER ---");
                Console.WriteLine(errorMsg);
                Console.WriteLine(ex.StackTrace);
                
                return StatusCode(500, errorMsg);
            }
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

        [HttpGet]
        // [Authorize(Roles = "Staff")]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 20)
        {
            var result = await _donHangService.GetAllDonHangsAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPatch("{maDH}/status")]
        public async Task<IActionResult> UpdateDonHangStatus(Guid maDH, [FromBody] string newStatus)
        {
            var success = await _donHangService.UpdateDonHangStatusAsync(maDH, newStatus);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("sync-sales-count")]
        public async Task<IActionResult> SyncSalesCount()
        {
            var success = await _donHangService.SyncSalesCountAsync();
            if (!success) return StatusCode(500, "Failed to sync sales count");
            return Ok("Sales count synced successfully");
        }
    }
}