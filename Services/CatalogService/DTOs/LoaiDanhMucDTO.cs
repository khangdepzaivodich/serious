namespace CatalogService.DTOs
{
    public class LoaiDanhMucDTO
    {
        public required Guid MaLDM { get; set; }
        public required string TenLDM { get; set; }
        public List<DanhMucDTO> DanhMucs { get; set; } = new();
    }
}
