namespace CatalogService.DTOs
{
    public class SanPhamCreateDTO
    {
        public Guid MaDM { get; set; }
        public required string TenSP { get; set; }
        public string? MoTa { get; set; }
    }
}
