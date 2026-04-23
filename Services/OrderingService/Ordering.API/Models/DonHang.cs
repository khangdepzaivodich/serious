using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderingService.Ordering.API.Models
{
    public class DonHang
    {
        [Key]
        public Guid MaDH { get; set; } = Guid.NewGuid();

        [Required]
        public Guid MaTK { get; set; }

        [Required]
        [MaxLength(150)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SoDienThoai { get; set; } = string.Empty;

        public Guid? MaGG { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(255)]
        public string DiaChiGiaoHang { get; set; } = string.Empty;

        [Required]
        public decimal TongTien { get; set; }

        [MaxLength(50)]
        public string TrangThaiDH { get; set; } = "ChoXacNhan"; 

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }
}
