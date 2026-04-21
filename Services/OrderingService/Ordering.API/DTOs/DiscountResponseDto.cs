using System;

namespace OrderingService.Ordering.API.DTOs
{
    // DTO để nhận response từ Discount API
    public class DiscountResponseDto
    {
        public Guid MaGG { get; set; }
        public string MaCode { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty; // "PhanTram", "Tien"
        public decimal SoTien { get; set; }
        public decimal? GiaTriGiamToiDa { get; set; }
    }
}
