using CatalogService.CatalogServices.Interfaces;
using CatalogService.Data;
using Microsoft.EntityFrameworkCore;
using CatalogService.DTOs;
using CatalogService.Models;

namespace CatalogService.CatalogServices.Implementations
{
    public class SanPhamService : ISanPhamService
    {
        private readonly AppDbContext _context;

        public SanPhamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SanPhamDTO>> GetAllSanPhamsAsync()
        {
            var sanPhams = await _context.SanPhams
                .Include(sp => sp.ChiTietSanPhams) // Eager loading variants
                .ToListAsync();

            // Mapping Entity to DTO (You can also use AutoMapper for this)
            return sanPhams.Select(sp => new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                MoTa = sp.MoTa ?? "",
                ChiTietSanPhams = sp.ChiTietSanPhams.Select(ct => new ChiTietSanPhamDTO
                {
                    MaCTSP = ct.MaCTSP,
                    Mau = ct.Mau,
                    KichCo = ct.KichCo,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong,
                    Anh = ct.Anh ?? ""
                }).ToList()
            });
        }

        public async Task<SanPhamDTO> GetSanPhamByIdAsync(Guid id)
        {
            var sp = await _context.SanPhams
                .Include(x => x.ChiTietSanPhams)
                .FirstOrDefaultAsync(x => x.MaSP == id);

            if (sp == null) return null;

            return new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                MoTa = sp.MoTa,
                ChiTietSanPhams = sp.ChiTietSanPhams.Select(ct => new ChiTietSanPhamDTO
                {
                    MaCTSP = ct.MaCTSP,
                    Mau = ct.Mau,
                    KichCo = ct.KichCo,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong,
                    Anh = ct.Anh ?? ""
                }).ToList()
            };
        }

        public async Task<SanPhamDTO> CreateSanPhamAsync(SanPhamCreateDTO createDto)
        {
            var newSanPham = new SanPham
            {
                MaSP = Guid.NewGuid(),
                MaDM = createDto.MaDM,
                TenSP = createDto.TenSP,
                MoTa = createDto.MoTa
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();

            return new SanPhamDTO
            {
                MaSP = newSanPham.MaSP,
                MaDM = newSanPham.MaDM,
                TenSP = newSanPham.TenSP,
                MoTa = newSanPham.MoTa
            };
        }

        public async Task<bool> UpdateSanPhamAsync(Guid id, SanPhamCreateDTO updateDto)
        {
            var existingSanPham = await _context.SanPhams.FindAsync(id);
            if (existingSanPham == null) return false;

            existingSanPham.MaDM = updateDto.MaDM;
            existingSanPham.TenSP = updateDto.TenSP;
            existingSanPham.MoTa = updateDto.MoTa;

            _context.SanPhams.Update(existingSanPham);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSanPhamAsync(Guid id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null) return false;

            _context.SanPhams.Remove(sanPham);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}