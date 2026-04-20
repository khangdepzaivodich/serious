namespace CatalogService.DTOs
{
    public class ChiTietSanPhamCreateUpdateDTO
    {
        public required Guid MaSP { get; set; }
        public string? Mau { get; set; }
        public string? KichCo { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public string? Anh { get; set; }
    }
}
