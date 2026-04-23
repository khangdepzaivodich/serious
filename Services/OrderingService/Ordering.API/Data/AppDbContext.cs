using Microsoft.EntityFrameworkCore;
using OrderingService.Ordering.API.Models;

namespace OrderingService.Ordering.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<DonHang> DonHangs { get; set; } = null!;
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.DonHang)
                .WithMany(d => d.ChiTietDonHangs)
                .HasForeignKey(c => c.MaDH);

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(c => c.Gia_LuuTru)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.TongTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.HoTen)
                .HasMaxLength(150);

            modelBuilder.Entity<DonHang>()
                .Property(d => d.SoDienThoai)
                .HasMaxLength(20);
        }
    }
}
