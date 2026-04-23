using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderingService.Ordering.API.DTOs
{
    public class CreateDonHangRequest
    {
        [Required]
        public Guid MaTK { get; set; }

        [Required]
        [MinLength(2)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(0|\+84)(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "SoDienThoai is invalid")]
        public string SoDienThoai { get; set; } = string.Empty;

        public Guid? MaGG { get; set; }

        [Required]
        [MinLength(5)]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one cart item is required")]
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
        public string? Anh_LuuTru { get; set; }
    }
}