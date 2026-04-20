using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Models
{
    public class SanPham
    {
        [Key]
        public required Guid MaSP { get; set; }

        [Required]
        public required Guid MaDM { get; set; }
             
        [Required]
        [StringLength(255)]
        public required string TenSP { get; set; }

        public string? MoTa { get; set; }

        [ForeignKey("MaDM")]
        public virtual DanhMuc? DanhMuc { get; set; }

        public virtual ICollection<ChiTietSanPham> ChiTietSanPhams { get; set; }

        public SanPham()
        {
            ChiTietSanPhams = new HashSet<ChiTietSanPham>();
        }
    }
}