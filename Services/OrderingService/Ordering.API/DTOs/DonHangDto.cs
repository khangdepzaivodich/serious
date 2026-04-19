using System;
using System.Collections.Generic;

namespace OrderingService.Ordering.API.DTOs
{
    public class DonHangDto
    {
        public Guid MaDH { get; set; }
        public Guid MaTK { get; set; }
        public Guid? MaGG { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThaiDH { get; set; } = string.Empty;
        public string DiaChiGiaoHang { get; set; } = string.Empty;
        public List<ChiTietDonHangDto> ChiTietDonHangs { get; set; } = new();
    }
}
