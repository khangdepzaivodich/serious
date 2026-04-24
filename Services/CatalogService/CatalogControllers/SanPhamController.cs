using Microsoft.AspNetCore.Mvc;
using CatalogService.CatalogServices.Interfaces;
using CatalogService.DTOs;

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
        public async Task<IActionResult> GetPaged([FromQuery] SanPhamPaginationDTO paginationDto)
        {
            var (data, totalCount) = await _service.GetPagedSanPhamsAsync(paginationDto);

            return Ok(new
            {
                TotalCount = totalCount,
                paginationDto.PageNumber,
                paginationDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / paginationDto.PageSize),
                Data = data
            });
        }
        // GET: api/sanpham/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sanPham = await _service.GetSanPhamByIdAsync(id);
            if (sanPham == null)
                return NotFound();

            return Ok(sanPham);
        }

        // GET: api/sanpham/slug/{slug}
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var sanPham = await _service.GetSanPhamBySlugAsync(slug);
            if (sanPham == null)
                return NotFound();

            return Ok(sanPham);
        }

        // POST: api/sanpham
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SanPhamCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateSanPhamAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.MaSP }, created);
        }

        // PUT: api/sanpham/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SanPhamCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _service.UpdateSanPhamAsync(id, dto);
            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteSanPhamAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("update-sales-count")]
        public async Task<IActionResult> UpdateSalesCount([FromBody] List<SalesUpdateDto> salesUpdates)
        {
            await _service.IncrementLuotBanAsync(salesUpdates);
            return Ok();
        }

        [HttpPost("sync-sales-count")]
        public async Task<IActionResult> SyncSalesCount([FromBody] List<SalesUpdateDto> salesUpdates, [FromQuery] bool isFullSync = true)
        {
            var success = await _service.SyncLuotBanAsync(salesUpdates, isFullSync);
            if (!success) return StatusCode(500, "Failed to sync sales count");
            return Ok("Sales count synced successfully");
        }
    }
}