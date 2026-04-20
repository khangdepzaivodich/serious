namespace CatalogService.DTOs
{
    public class ChiTietSanPhamDTO
    {
        public Guid MaCTSP { get; set; }
        public Guid MaSP { get; set; }
        public required string Mau { get; set; }
        public required string KichCo { get; set; }
        public required decimal Gia { get; set; }
        public required int SoLuong { get; set; }
        public string? Anh { get; set; }
    }
}
