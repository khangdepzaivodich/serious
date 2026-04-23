using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderingService.Ordering.API.Models
{
    public class ChiTietDonHang
    {
        [Key]
        public Guid MaCTDH { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaDH { get; set; }

        [Required]
        public Guid MaCTSP { get; set; }

        [Required]
        public string TenSP_LuuTru { get; set; } = string.Empty;

        public string? Mau_LuuTru { get; set; }

        public string? KichCo_LuuTru { get; set; }

        [Required]
        public decimal Gia_LuuTru { get; set; }

        [Required]
        public int SoLuong { get; set; }

        public string? Anh_LuuTru { get; set; }

        [ForeignKey("MaDH")]
        public DonHang DonHang { get; set; } = null!;
    }
}
