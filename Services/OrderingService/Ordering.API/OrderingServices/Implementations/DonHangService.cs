using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderingService.Ordering.API.Data;
using OrderingService.Ordering.API.DTOs;
using OrderingService.Ordering.API.Models;
using OrderingService.Ordering.API.OrderingServices.Interfaces;

namespace OrderingService.Ordering.API.OrderingServices.Implementations
{
    public class DonHangService : IDonHangService
    {
        private readonly AppDbContext _context;

        public DonHangService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DonHangDto> CreateDonHangAsync(CreateDonHangRequest request)
        {
            var donHang = new DonHang
            {
                MaTK = request.MaTK,
                MaGG = request.MaGG,
                DiaChiGiaoHang = request.DiaChiGiaoHang,
                TongTien = request.ChiTietDonHangs.Sum(x => x.SoLuong * x.Gia_LuuTru),
                ChiTietDonHangs = request.ChiTietDonHangs.Select(x => new ChiTietDonHang
                {
                    MaCTSP = x.MaCTSP,
                    TenSP_LuuTru = x.TenSP_LuuTru,
                    Mau_LuuTru = x.Mau_LuuTru,
                    KichCo_LuuTru = x.KichCo_LuuTru,
                    SoLuong = x.SoLuong,
                    Gia_LuuTru = x.Gia_LuuTru
                }).ToList()
            };

            _context.DonHangs.Add(donHang);
            await _context.SaveChangesAsync();

            return MapToDto(donHang);
        }

        public async Task<DonHangDto?> GetDonHangByIdAsync(Guid maDH)
        {
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDH == maDH);

            if (donHang == null) return null;

            return MapToDto(donHang);
        }

        public async Task<IEnumerable<DonHangDto>> GetDonHangsByUserIdAsync(Guid maTK)
        {
            var donHangs = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.MaTK == maTK)
                .OrderByDescending(d => d.NgayDat)
                .ToListAsync();

            return donHangs.Select(MapToDto);
        }

        public async Task<bool> UpdateDonHangStatusAsync(Guid maDH, string newStatus)
        {
            var donHang = await _context.DonHangs.FindAsync(maDH);
            if (donHang == null) return false;

            donHang.TrangThaiDH = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedDonHangResult> GetAllDonHangsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.DonHangs.Include(d => d.ChiTietDonHangs).AsQueryable();
            var total = await query.CountAsync();

            var items = await query.OrderByDescending(d => d.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedDonHangResult
            {
                Items = items.Select(MapToDto),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static DonHangDto MapToDto(DonHang donHang)
        {
            return new DonHangDto
            {
                MaDH = donHang.MaDH,
                MaTK = donHang.MaTK,
                MaGG = donHang.MaGG,
                NgayDat = donHang.NgayDat,
                TongTien = donHang.TongTien,
                TrangThaiDH = donHang.TrangThaiDH,
                DiaChiGiaoHang = donHang.DiaChiGiaoHang,
                ChiTietDonHangs = donHang.ChiTietDonHangs.Select(c => new ChiTietDonHangDto
                {
                    MaCTDH = c.MaCTDH,
                    MaCTSP = c.MaCTSP,
                    TenSP_LuuTru = c.TenSP_LuuTru,
                    Mau_LuuTru = c.Mau_LuuTru,
                    KichCo_LuuTru = c.KichCo_LuuTru,
                    SoLuong = c.SoLuong,
                    Gia_LuuTru = c.Gia_LuuTru
                }).ToList()
            };
        }
    }
}