using CatalogService.CatalogServices.Interfaces;
using CatalogService.Data;
using Microsoft.EntityFrameworkCore;
using CatalogService.DTOs;
using CatalogService.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CatalogService.CatalogServices.Implementations
{
    public class SanPhamService : ISanPhamService
    {
        private readonly AppDbContext _context;

        public SanPhamService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<SanPhamDTO> Data, int TotalCount)> GetPagedSanPhamsAsync(SanPhamPaginationDTO paginationDto)
        {
            var query = _context.SanPhams
            .Include(sp => sp.ChiTietSanPhams)
            .Include(sp => sp.DanhMuc!)
                .ThenInclude(dm => dm!.LoaiDanhMuc) 
            .AsQueryable();
            // FILTER: DanhMuc
            if (paginationDto.MaDM.HasValue)
            {
                query = query.Where(sp => sp.MaDM == paginationDto.MaDM.Value);
            }

            // FILTER: LoaiDanhMuc
            if (paginationDto.MaLDM.HasValue)
            {
                query = query.Where(sp =>
                    sp.DanhMuc != null &&
                    sp.DanhMuc.MaLDM == paginationDto.MaLDM.Value);
            }

            // FILTER: keyword
            if (!string.IsNullOrWhiteSpace(paginationDto.Keyword))
            {
                var kw = paginationDto.Keyword.ToLower();
                query = query.Where(sp => sp.TenSP.ToLower().Contains(kw));
            }

            // FILTER: Price
            if (paginationDto.MinPrice.HasValue || paginationDto.MaxPrice.HasValue)
            {
                query = query.Where(sp => sp.ChiTietSanPhams.Any(ct => 
                    (!paginationDto.MinPrice.HasValue || ct.Gia >= paginationDto.MinPrice.Value) &&
                    (!paginationDto.MaxPrice.HasValue || ct.Gia <= paginationDto.MaxPrice.Value)
                ));
            }

            var totalCount = await query.CountAsync();

            var sanPhams = await query
                .OrderBy(sp => sp.TenSP)
                .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize)
                .ToListAsync();

            var data = sanPhams.Select(sp => new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                MoTa = sp.MoTa ?? "",
                ChiTietSanPhams = [.. sp.ChiTietSanPhams.Select(ct => new ChiTietSanPhamDTO
                {
                    MaCTSP = ct.MaCTSP,
                    Mau = ct.Mau,
                    KichCo = ct.KichCo,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong,
                    Anh = ct.Anh ?? ""
                })]
            });

            return (data, totalCount);
        }


        public async Task<SanPhamDTO> GetSanPhamByIdAsync(Guid id)
        {
            var sp = await _context.SanPhams
                .Include(x => x.ChiTietSanPhams)
                .FirstOrDefaultAsync(x => x.MaSP == id);

            if (sp == null)
                return new SanPhamDTO
                {
                    MaSP = Guid.Empty,
                    MaDM = Guid.Empty,          
                    TenSP = "",
                    MoTa = "",
                    ChiTietSanPhams = new List<ChiTietSanPhamDTO>()
                };

            return new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                MoTa = sp.MoTa ?? "",
                ChiTietSanPhams = [.. sp.ChiTietSanPhams.Select(ct => new ChiTietSanPhamDTO
                {
                    MaCTSP = ct.MaCTSP,
                    Mau = ct.Mau,
                    KichCo = ct.KichCo,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong,
                    Anh = ct.Anh ?? ""
                })]
            };
        }
        public async Task<SanPhamDTO?> GetSanPhamBySlugAsync(string slug)
        {
            var sanPhams = await _context.SanPhams
                .Include(x => x.ChiTietSanPhams)
                .Include(x => x.DanhMuc!)
                    .ThenInclude(dm => dm!.LoaiDanhMuc)
                .ToListAsync();

            var sp = sanPhams.FirstOrDefault(x => GenerateSlug(x.TenSP) == slug);

            if (sp == null)
                return null;

            return new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                MoTa = sp.MoTa ?? "",
                ChiTietSanPhams = [.. sp.ChiTietSanPhams.Select(ct => new ChiTietSanPhamDTO
                {
                    MaCTSP = ct.MaCTSP,
                    Mau = ct.Mau,
                    KichCo = ct.KichCo,
                    Gia = ct.Gia,
                    SoLuong = ct.SoLuong,
                    Anh = ct.Anh ?? ""
                })]
            };
        }

        private static string GenerateSlug(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            var result = sb.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace("đ", "d").Replace("Đ", "d");
            result = result.ToLowerInvariant();
            result = Regex.Replace(result, @"[^a-z0-9\s-]", "");
            result = Regex.Replace(result, @"[\s]+", "-");
            result = Regex.Replace(result, @"-{2,}", "-");
            result = result.Trim('-');
            return result;
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