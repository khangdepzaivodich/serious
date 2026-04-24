using System;

namespace DiscountService.Discount.API.DTOs
{
    public class MaGiamGiaDto
    {
        public Guid MaGG { get; set; }
        public string MaCode { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public decimal? DonHangToiThieu { get; set; }
        public decimal? GiaTriGiamToiDa { get; set; }
        public int SoLuong { get; set; }
        public DateTime HanSuDung { get; set; }
        public string ApDungCho { get; set; } = "TatCa";
        public Guid? MaLDM { get; set; }
        public Guid? MaDM { get; set; }
        public Guid? MaSP { get; set; }
        public List<Guid> MaSPs { get; set; } = new();
    }

    public class CreateMaGiamGiaRequest
    {
        public string MaCode { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public decimal? DonHangToiThieu { get; set; }
        public decimal? GiaTriGiamToiDa { get; set; }
        public int SoLuong { get; set; }
        public DateTime HanSuDung { get; set; }
        public string ApDungCho { get; set; } = "TatCa";
        public Guid? MaLDM { get; set; }
        public Guid? MaDM { get; set; }
        public Guid? MaSP { get; set; }
        public List<Guid> MaSPs { get; set; } = new();
    }
}
