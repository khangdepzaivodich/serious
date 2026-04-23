using System;

namespace OrderingService.Ordering.API.DTOs
{
    public class ChiTietDonHangDto
    {
        public Guid MaCTDH { get; set; }
        public Guid MaCTSP { get; set; }
        public string TenSP_LuuTru { get; set; } = string.Empty;
        public string? Mau_LuuTru { get; set; }
        public string? KichCo_LuuTru { get; set; }
        public decimal Gia_LuuTru { get; set; }
        public int SoLuong { get; set; }
        public string? Anh_LuuTru { get; set; }
    }
}