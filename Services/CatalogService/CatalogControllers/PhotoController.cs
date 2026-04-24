using Microsoft.AspNetCore.Mvc;
using CatalogService.CatalogServices;

namespace CatalogService.CatalogControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoService _photoService;

        public PhotoController(IPhotoService photoService)
        {
            _photoService = photoService;
        }

        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            return Ok(new { url = result.SecureUrl.AbsoluteUri });
        }
    }
}
