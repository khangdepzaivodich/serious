using Microsoft.AspNetCore.Mvc;
using CatalogService.DTOs;
using CatalogService.CatalogServices.Interfaces;
namespace CatalogService.CatalogControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiDanhMucController : ControllerBase
    {
        private readonly ILoaiDanhMucService _service;

        public LoaiDanhMucController(ILoaiDanhMucService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LoaiDanhMucCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.MaLDM }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LoaiDanhMucCreateUpdateDTO dto)
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
