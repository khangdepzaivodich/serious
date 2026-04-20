namespace CatalogService.DTOs
{
    public class DanhMucCreateUpdateDTO
    {
        public required Guid MaLDM { get; set; }
        public required string TenDM { get; set; }
    }
}
