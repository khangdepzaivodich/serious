using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly HttpClient _discountClient;

        public DonHangService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _discountClient = httpClientFactory.CreateClient("DiscountAPI");
        }

        public async Task<DonHangDto> CreateDonHangAsync(CreateDonHangRequest request)
        {
            decimal tongTienHang = request.ChiTietDonHangs.Sum(x => x.SoLuong * x.Gia_LuuTru);
            decimal tongTienThanhToan = tongTienHang;

            // CALL DISCOUNT SERVICE TO VERIFY AND CALCULATE DISCOUNT
            if (request.MaGG.HasValue)
            {
                try
                {
                    var discountInfo = await _discountClient.GetFromJsonAsync<DiscountResponseDto>($"api/magiamgia/{request.MaGG.Value}");
                    if (discountInfo != null)
                    {
                        if (discountInfo.Loai == "PhanTram")
                        {
                            decimal giamGia = tongTienHang * (discountInfo.SoTien / 100m);
                            if (discountInfo.GiaTriGiamToiDa.HasValue && giamGia > discountInfo.GiaTriGiamToiDa.Value)
                            {
                                giamGia = discountInfo.GiaTriGiamToiDa.Value;
                            }
                            tongTienThanhToan -= giamGia;
                        }
                        else 
                        {
                            // Loại giảm tiền trực tiếp
                            tongTienThanhToan -= discountInfo.SoTien;
                        }
                        
                        // Đảm bảo không âm
                        if (tongTienThanhToan < 0) tongTienThanhToan = 0;

                        // Báo cho Discount Service trừ số lượng (Sử dụng mã)
                        await _discountClient.PatchAsync($"api/magiamgia/use/{discountInfo.MaCode}", null);
                    }
                }
                catch
                {
                    // Lỗi gọi sang Discount Service (Bỏ qua giảm giá hoặc có thể throw exception tùy logic)
                }
            }

            var donHang = new DonHang
            {
                MaTK = request.MaTK,
                MaGG = request.MaGG,
                DiaChiGiaoHang = request.DiaChiGiaoHang,
                TongTien = tongTienThanhToan,
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