using System.Net.Http.Json;

namespace BasketService.BasketAPI.Services
{
    public interface ICatalogService
    {
        Task<CatalogItem?> GetProductAsync(string productId);
    }

    public class CatalogItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CatalogItemDTO
    {
        public Guid MaCTSP { get; set; }
        public Guid MaSP { get; set; }
        public string Mau { get; set; } = string.Empty;
        public string KichCo { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public string? Anh { get; set; }
    }

    public class CatalogApiService : ICatalogService
    {
        private readonly HttpClient _httpClient;

        public CatalogApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CatalogItem?> GetProductAsync(string productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/ChiTietSanPham/{productId}");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<CatalogItemDTO>();
                    if (dto != null)
                    {
                        return new CatalogItem 
                        { 
                            Id = dto.MaCTSP.ToString(), 
                            Price = dto.Gia, 
                            ImageUrl = dto.Anh,
                            Name = $"{dto.Mau} - {dto.KichCo}" // Ghép tạm thông tin màu và kích cỡ vì API ChiTietSanPham không trả về TenSP
                        };
                    }
                }
            }
            catch (Exception)
            {
                // Ignored
            }
            return null;
        }
    }
}
