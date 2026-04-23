using CatalogService.CatalogServices.Interfaces;
using CatalogService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.CatalogControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamService _service;

        public SanPhamController(ISanPhamService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetAllSanPhamsAsync());

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _service.GetSanPhamByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SanPhamCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _service.CreateSanPhamAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = result.MaSP }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SanPhamCreateDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            return await _service.UpdateSanPhamAsync(id, dto)
                ? NoContent()
                : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
            => await _service.DeleteSanPhamAsync(id) ? NoContent() : NotFound();
    }
}