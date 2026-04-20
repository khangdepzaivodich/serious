using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Models
{
    public class LoaiDanhMuc
    {
        [Key]
        public required Guid MaLDM { get; set; }

        [Required]
        public required string TenLDM { get; set; }

        public virtual ICollection<DanhMuc> DanhMucs { get; set; }

        public LoaiDanhMuc()
        {
            DanhMucs = new HashSet<DanhMuc>();
        }
    }
}