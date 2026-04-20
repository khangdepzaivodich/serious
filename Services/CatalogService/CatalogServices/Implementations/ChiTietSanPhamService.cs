using CatalogService.CatalogServices.Interfaces;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogService.CatalogServices.Implementations
{ 
    public class ChiTietSanPhamService : IChiTietSanPhamService
    {
        private readonly AppDbContext _context;

        public ChiTietSanPhamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChiTietSanPhamDTO>> GetBySanPhamIdAsync(Guid maSP)
        {
            return await _context.ChiTietSanPhams
                .Where(x => x.MaSP == maSP)
                .Select(x => new ChiTietSanPhamDTO
                {
                    MaCTSP = x.MaCTSP,
                    MaSP = x.MaSP,
                    Mau = x.Mau,
                    KichCo = x.KichCo,
                    Gia = x.Gia,
                    SoLuong = x.SoLuong,
                    Anh = x.Anh
                })
                .ToListAsync();
        }

        public async Task<ChiTietSanPhamDTO?> GetByIdAsync(Guid id)
        {
            var entity = await _context.ChiTietSanPhams.FindAsync(id);
            if (entity == null) return null;

            return new ChiTietSanPhamDTO
            {
                MaCTSP = entity.MaCTSP,
                MaSP = entity.MaSP,
                Mau = entity.Mau,
                KichCo = entity.KichCo,
                Gia = entity.Gia,
                SoLuong = entity.SoLuong,
                Anh = entity.Anh
            };
        }

        public async Task<ChiTietSanPhamDTO> CreateAsync(ChiTietSanPhamCreateUpdateDTO dto)
        {
            var entity = new ChiTietSanPham
            {
                MaCTSP = Guid.NewGuid(),
                MaSP = dto.MaSP,
                Mau = dto.Mau ?? "",
                KichCo = dto.KichCo ?? "",
                Gia = dto.Gia,
                SoLuong = dto.SoLuong,
                Anh = dto.Anh
            };

            _context.ChiTietSanPhams.Add(entity);
            await _context.SaveChangesAsync();

            return new ChiTietSanPhamDTO
            {
                MaCTSP = entity.MaCTSP,
                MaSP = entity.MaSP,
                Mau = entity.Mau,
                KichCo = entity.KichCo,
                Gia = entity.Gia,
                SoLuong = entity.SoLuong,
                Anh = entity.Anh
            };
        }

        public async Task<bool> UpdateAsync(Guid id, ChiTietSanPhamCreateUpdateDTO dto)
        {
            var entity = await _context.ChiTietSanPhams.FindAsync(id);
            if (entity == null) return false;

            entity.Mau = dto.Mau;
            entity.KichCo = dto.KichCo;
            entity.Gia = dto.Gia;
            entity.SoLuong = dto.SoLuong;
            entity.Anh = dto.Anh;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStockAsync(Guid id, int quantityChange)
        {
            var entity = await _context.ChiTietSanPhams.FindAsync(id);
            if (entity == null) return false;

            entity.SoLuong += quantityChange;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.ChiTietSanPhams.FindAsync(id);
            if (entity == null) return false;

            _context.ChiTietSanPhams.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
