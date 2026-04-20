using Microsoft.AspNetCore.Mvc;
using CatalogService.DTOs;
using CatalogService.CatalogServices.Interfaces;

namespace CatalogService.CatalogControllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChiTietSanPhamController : ControllerBase
    {
        private readonly IChiTietSanPhamService _service;

        public ChiTietSanPhamController(IChiTietSanPhamService service)
        {
            _service = service;
        }

        [HttpGet("by-sanpham/{maSP}")]
        public async Task<IActionResult> GetBySanPham(Guid maSP)
            => Ok(await _service.GetBySanPhamIdAsync(maSP));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChiTietSanPhamCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.MaCTSP }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChiTietSanPhamCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _service.UpdateAsync(id, dto)
                ? NoContent()
                : NotFound();
        }

        [HttpPatch("{id}/stock")]
        public async Task<IActionResult> UpdateStock(Guid id, [FromQuery] int change)
            => await _service.UpdateStockAsync(id, change) ? NoContent() : NotFound();

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
