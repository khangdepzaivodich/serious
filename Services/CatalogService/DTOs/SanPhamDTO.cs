namespace CatalogService.DTOs
{   
    public class SanPhamDTO
    {
        public Guid MaSP { get; set; }
        public Guid MaDM { get; set; }
        public required string TenSP { get; set; }
        public string? Slug { get; set; }
        public string? MoTa { get; set; }
        public int LuotBan { get; set; }
        public List<ChiTietSanPhamDTO> ChiTietSanPhams { get; set; } = new List<ChiTietSanPhamDTO>();
    }
}
