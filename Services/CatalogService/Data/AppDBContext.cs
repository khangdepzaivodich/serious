using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets represent the tables in your database
        public DbSet<LoaiDanhMuc> LoaiDanhMucs { get; set; }
        public DbSet<DanhMuc> DanhMucs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<ChiTietSanPham> ChiTietSanPhams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Relationship: LoaiDanhMuc -> DanhMuc (1-to-Many)
            modelBuilder.Entity<LoaiDanhMuc>()
                .HasMany(ldm => ldm.DanhMucs)
                .WithOne(dm => dm.LoaiDanhMuc)
                .HasForeignKey(dm => dm.MaLDM)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Relationship: DanhMuc -> SanPham (1-to-Many)
            modelBuilder.Entity<DanhMuc>()
                .HasMany(dm => dm.SanPhams)
                .WithOne(sp => sp.DanhMuc)
                .HasForeignKey(sp => sp.MaDM)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Relationship: SanPham -> ChiTietSanPham (1-to-Many)
            modelBuilder.Entity<SanPham>()
                .HasMany(sp => sp.ChiTietSanPhams)
                .WithOne(ct => ct.SanPham)
                .HasForeignKey(ct => ct.MaSP)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}