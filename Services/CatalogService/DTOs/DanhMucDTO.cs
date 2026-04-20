namespace CatalogService.DTOs
{
    public class DanhMucDTO
    {
        public required Guid MaDM { get; set; }
        public required Guid MaLDM { get; set; }
        public required string TenDM { get; set; }
    }
}
