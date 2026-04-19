using System;
using System.Collections.Generic;

namespace OrderingService.Ordering.API.DTOs
{
    public class CreateDonHangRequest
    {
        public Guid MaTK { get; set; }
        public Guid? MaGG { get; set; }
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public List<CreateChiTietDonHangRequest> ChiTietDonHangs { get; set; } = new();
    }

    public class CreateChiTietDonHangRequest
    {
        public Guid MaCTSP { get; set; }
        public string TenSP_LuuTru { get; set; } = string.Empty;
        public string? Mau_LuuTru { get; set; }
        public string? KichCo_LuuTru { get; set; }
        public decimal Gia_LuuTru { get; set; }
        public int SoLuong { get; set; }
    }
}