using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Models
{
    public class ChiTietSanPham
    {
        [Key]
        public required Guid MaCTSP { get; set; }

        [Required]
        public required Guid MaSP { get; set; }

        public required string Mau { get; set; } // Color

        public required string KichCo { get; set; } // Size

        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; } // Priceg

        public int SoLuong { get; set; } // Quantity

        public string? Anh { get; set; } // Image URL/Path

        // Navigation Property
        [ForeignKey("MaSP")]
        public virtual SanPham SanPham { get; set; } = null!;
    }
}