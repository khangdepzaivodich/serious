using Microsoft.AspNetCore.Mvc;
using CatalogService.DTOs;
using CatalogService.CatalogServices.Interfaces;
namespace CatalogService.CatalogControllers
{

    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = "ADMIN")] // Enable this to restrict category management to Admins
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucService _service;

        public DanhMucController(IDanhMucService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("by-loaidanhmuc/{maLDM}")]
        public async Task<IActionResult> GetByLoaiDanhMuc(Guid maLDM)
            => Ok(await _service.GetByLoaiDanhMucIdAsync(maLDM));

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DanhMucCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.MaDM }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DanhMucCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _service.UpdateAsync(id, dto)
                ? NoContent()
                : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
