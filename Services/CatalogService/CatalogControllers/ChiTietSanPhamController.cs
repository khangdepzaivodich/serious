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
        private readonly CatalogServices.IPhotoService _photoService;

        public ChiTietSanPhamController(IChiTietSanPhamService service, CatalogServices.IPhotoService photoService)
        {
            _service = service;
            _photoService = photoService;
        }

        [HttpPost("{id}/photo")]
        public async Task<IActionResult> AddPhoto(Guid id, IFormFile file)
        {
            var variant = await _service.GetByIdAsync(id);
            if (variant == null) return NotFound("Variant not found");

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            // Cập nhật URL ảnh vào Variant
            // Giả sử service có hàm cập nhật ảnh, nếu không mình sẽ sửa trực tiếp hoặc dùng UpdateAsync
            var updateDto = new ChiTietSanPhamCreateUpdateDTO
            {
                MaSP = variant.MaSP,
                Mau = variant.Mau,
                KichCo = variant.KichCo,
                Gia = variant.Gia,
                SoLuong = variant.SoLuong,
                Anh = result.SecureUrl.AbsoluteUri
            };

            await _service.UpdateAsync(id, updateDto);

            return Ok(new { url = result.SecureUrl.AbsoluteUri });
        }

        [HttpGet("by-sanpham/{maSP}")]
        public async Task<IActionResult> GetBySanPham(Guid maSP)
            => Ok(await _service.GetBySanPhamIdAsync(maSP));

        [HttpGet("/api/sanpham/{maSP}/variants")]
        public async Task<IActionResult> GetBySanPhamLegacy(Guid maSP)
            => Ok(await _service.GetBySanPhamIdAsync(maSP));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("/api/sanpham/variants/{id}")]
        public async Task<IActionResult> GetLegacy(Guid id)
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

        [HttpPost("/api/sanpham/variants")]
        public async Task<IActionResult> CreateLegacy([FromBody] ChiTietSanPhamCreateUpdateDTO dto)
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

        [HttpPut("/api/sanpham/variants/{id}")]
        public async Task<IActionResult> UpdateLegacy(Guid id, [FromBody] ChiTietSanPhamCreateUpdateDTO dto)
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

        [HttpDelete("/api/sanpham/variants/{id}")]
        public async Task<IActionResult> DeleteLegacy(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
