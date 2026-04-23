using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Models
{
    public class DanhMuc
    {
        [Key]
        public required Guid MaDM { get; set; }

        [Required]
        public required Guid MaLDM { get; set; }

        [Required]
        public required string TenDM { get; set; }

        [ForeignKey("MaLDM")]
        public virtual LoaiDanhMuc? LoaiDanhMuc { get; set; } 

        public virtual ICollection<SanPham> SanPhams { get; set; }

        public DanhMuc()
        {
            SanPhams = new HashSet<SanPham>();
        }
    }
}