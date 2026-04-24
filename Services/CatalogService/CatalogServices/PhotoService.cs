using CatalogService.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace CatalogService.CatalogServices
{
    public class PhotoService : IPhotoService
    {
        private readonly CloudinarySettings _settings;
        private readonly Lazy<Cloudinary> _cloudinary;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            _settings = config.Value;
            _cloudinary = new Lazy<Cloudinary>(CreateCloudinary);
        }

        private Cloudinary CreateCloudinary()
        {
            if (string.IsNullOrWhiteSpace(_settings.CloudName)
                || string.IsNullOrWhiteSpace(_settings.ApiKey)
                || string.IsNullOrWhiteSpace(_settings.ApiSecret))
            {
                throw new InvalidOperationException("CloudinarySettings is missing. Please configure CloudName, ApiKey, and ApiSecret in appsettings.Development.json.");
            }

            var acc = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
            return new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                    Folder = "AE2-Catalog"
                };
                uploadResult = await _cloudinary.Value.UploadAsync(uploadParams);
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.Value.DestroyAsync(deleteParams);
        }
    }
}
