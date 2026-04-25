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

            // Dynamic sorting based on SortBy parameter
            IOrderedQueryable<SanPham> orderedQuery = paginationDto.SortBy switch
            {
                "best-seller" => query.OrderByDescending(sp => sp.LuotBan),
                "price-asc" => query.OrderBy(sp => sp.ChiTietSanPhams.Min(ct => ct.Gia)),
                "price-desc" => query.OrderByDescending(sp => sp.ChiTietSanPhams.Max(ct => ct.Gia)),
                "newest" => query.OrderByDescending(sp => sp.MaSP), // MaSP as proxy for creation order
                _ => query.OrderBy(sp => sp.TenSP)
            };

            var sanPhams = await orderedQuery
                .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
                .Take(paginationDto.PageSize)
                .ToListAsync();

            var data = sanPhams.Select(sp => new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                Slug = sp.Slug,
                MoTa = sp.MoTa ?? "",
                LuotBan = sp.LuotBan,
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
                Slug = sp.Slug,
                MoTa = sp.MoTa ?? "",
                LuotBan = sp.LuotBan,
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
            var sp = await _context.SanPhams
                .Include(x => x.ChiTietSanPhams)
                .Include(x => x.DanhMuc!)
                    .ThenInclude(dm => dm!.LoaiDanhMuc)
                .FirstOrDefaultAsync(x => x.Slug == slug);

            if (sp == null)
                return null;

            return new SanPhamDTO
            {
                MaSP = sp.MaSP,
                MaDM = sp.MaDM,
                TenSP = sp.TenSP,
                Slug = sp.Slug,
                MoTa = sp.MoTa ?? "",
                LuotBan = sp.LuotBan,
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
                Slug = GenerateSlug(createDto.TenSP),
                MoTa = createDto.MoTa
            };

            _context.SanPhams.Add(newSanPham);
            await _context.SaveChangesAsync();

            return new SanPhamDTO
            {
                MaSP = newSanPham.MaSP,
                MaDM = newSanPham.MaDM,
                TenSP = newSanPham.TenSP,
                Slug = newSanPham.Slug,
                MoTa = newSanPham.MoTa,
                LuotBan = newSanPham.LuotBan
            };
        }

        public async Task<bool> UpdateSanPhamAsync(Guid id, SanPhamCreateDTO updateDto)
        {
            var existingSanPham = await _context.SanPhams.FindAsync(id);
            if (existingSanPham == null) return false;

            existingSanPham.MaDM = updateDto.MaDM;
            existingSanPham.TenSP = updateDto.TenSP;
            existingSanPham.Slug = GenerateSlug(updateDto.TenSP);
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

        public async Task<bool> IncrementLuotBanAsync(List<SalesUpdateDto> salesUpdates)
        {
            if (salesUpdates == null || !salesUpdates.Any()) return true;

            foreach (var update in salesUpdates)
            {
                // Find MaSP from MaCTSP
                var ctsp = await _context.ChiTietSanPhams
                    .Include(x => x.SanPham)
                    .FirstOrDefaultAsync(x => x.MaCTSP == update.MaCTSP);

                if (ctsp != null && ctsp.SanPham != null)
                {
                    ctsp.SanPham.LuotBan += update.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SyncLuotBanAsync(List<SalesUpdateDto> salesUpdates, bool isFullSync = true)
        {
            try
            {
                if (isFullSync)
                {
                    // Chỉ reset khi là Full Sync (bấm nút đồng bộ thủ công)
                    var allProducts = await _context.SanPhams.ToListAsync();
                    foreach (var sp in allProducts)
                    {
                        sp.LuotBan = 0;
                    }
                }

                if (salesUpdates != null && salesUpdates.Any())
                {
                    // Set lại giá trị chính xác từ tổng đơn hàng đã hoàn tất
                    foreach (var update in salesUpdates)
                    {
                        Console.WriteLine($"[CATALOG SYNC] Processing: {update.ProductName} | ID: {update.MaCTSP} | Qty: {update.Quantity}");
                        
                        // 1. Thử tìm theo Mã Chi Tiết (MaCTSP)
                        var ctsp = await _context.ChiTietSanPhams
                            .Include(x => x.SanPham)
                            .FirstOrDefaultAsync(x => x.MaCTSP == update.MaCTSP);

                        if (ctsp != null && ctsp.SanPham != null)
                        {
                            ctsp.SanPham.LuotBan += update.Quantity;
                            Console.WriteLine($"[CATALOG SYNC] Updated by Variant ID: {ctsp.SanPham.TenSP}");
                            continue;
                        }

                        // 2. Thử tìm trực tiếp theo Mã Sản Phẩm (MaSP) - Phòng trường hợp Ordering gửi nhầm MaSP
                        var spByMa = await _context.SanPhams
                            .FirstOrDefaultAsync(x => x.MaSP == update.MaCTSP);
                        if (spByMa != null)
                        {
                            spByMa.LuotBan += update.Quantity;
                            Console.WriteLine($"[CATALOG SYNC] Updated by Product ID: {spByMa.TenSP}");
                            continue;
                        }

                        // 3. Thử tìm theo Tên Sản Phẩm (ProductName) - Cú chốt cuối cùng
                        if (!string.IsNullOrEmpty(update.ProductName))
                        {
                            var spByName = await _context.SanPhams
                                .FirstOrDefaultAsync(x => x.TenSP.ToLower().Contains(update.ProductName.ToLower()) || 
                                                         update.ProductName.ToLower().Contains(x.TenSP.ToLower()));
                            if (spByName != null)
                            {
                                spByName.LuotBan += update.Quantity;
                                Console.WriteLine($"[CATALOG SYNC] Updated by Name: {spByName.TenSP}");
                                continue;
                            }
                        }

                        Console.WriteLine($"[CATALOG SYNC] WARNING: Could not find product for {update.ProductName} ({update.MaCTSP})");
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("[CATALOG SYNC] Successfully synced sales counts.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CATALOG SYNC] ERROR: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
                return false;
            }
        }
    }
}