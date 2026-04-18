using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Services.Identity.API.Models
{
    public class User
    {
        [Key]
        public Guid MaTK { get; set; } // UUID -> Guid

        [Required]
        [StringLength(20)]
        public required string SoDienThoai { get; set; } // string -> string

        [Required]
        [EmailAddress]
        public required string Email { get; set; } // string -> string

        [Required]
        public required string MatKhauHash { get; set; } // string -> string

        public required string HoTen { get; set; } // string -> string

        public required string DiaChi { get; set; } // string -> string

        public string? VaiTro { get; set; } // string -> string

        public string? TrangThai { get; set; } // string -> string

        public DateTime NgayThangNamSinh { get; set; } // date -> DateTime

        public DateTime LastActiveAt { get; set; } // datetime -> DateTime
    }
}