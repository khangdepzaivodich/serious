using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly IHttpClientFactory _httpClientFactory;

        public DonHangService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<DonHangDto> CreateDonHangAsync(CreateDonHangRequest request)
        {
            if (request.ChiTietDonHangs == null || request.ChiTietDonHangs.Count == 0)
                throw new ArgumentException("At least one order item is required.", nameof(request));

            decimal subtotal = request.ChiTietDonHangs.Sum(x => x.SoLuong * x.Gia_LuuTru);
            decimal discountAmount = 0;

            if (request.MaGG.HasValue)
            {
                try
                {
                    var discountClient = _httpClientFactory.CreateClient("DiscountService");
                    var discount = await discountClient.GetFromJsonAsync<MaGiamGiaDto>($"api/MaGiamGia/{request.MaGG}");
                    if (discount != null && subtotal >= (discount.DonHangToiThieu ?? 0) && discount.HanSuDung >= DateTime.UtcNow)
                    {
                        if (discount.Loai == "PhanTram")
                        {
                            discountAmount = subtotal * discount.SoTien / 100m;
                            if (discount.GiaTriGiamToiDa.HasValue && discount.GiaTriGiamToiDa.Value > 0)
                            {
                                discountAmount = Math.Min(discountAmount, discount.GiaTriGiamToiDa.Value);
                            }
                        }
                        else if (discount.Loai == "Tien")
                        {
                            discountAmount = Math.Min(discount.SoTien, subtotal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ORDERING] Failed to fetch discount {request.MaGG}: {ex.Message}");
                }
            }

            var donHang = new DonHang
            {
                MaTK = request.MaTK,
                HoTen = request.HoTen,
                SoDienThoai = request.SoDienThoai,
                MaGG = request.MaGG,
                DiaChiGiaoHang = request.DiaChiGiaoHang,
                TongTien = Math.Max(subtotal - discountAmount, 0),
                ChiTietDonHangs = request.ChiTietDonHangs.Select(x => new ChiTietDonHang
                {
                    MaCTSP = x.MaCTSP,
                    TenSP_LuuTru = x.TenSP_LuuTru,
                    Mau_LuuTru = x.Mau_LuuTru,
                    KichCo_LuuTru = x.KichCo_LuuTru,
                    SoLuong = x.SoLuong,
                    Gia_LuuTru = x.Gia_LuuTru,
                    Anh_LuuTru = x.Anh_LuuTru
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
            var donHang = await _context.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.MaDH == maDH);

            if (donHang == null) return false;

            var oldStatus = donHang.TrangThaiDH;
            donHang.TrangThaiDH = newStatus;
            await _context.SaveChangesAsync();

            // Nếu trạng thái mới là "Hoàn tất" (hoặc các biến thể) và trạng thái cũ chưa phải hoàn tất
            bool isNewStatusCompleted = newStatus.ToLower().Replace(" ", "").Contains("hoantat") || 
                                        newStatus.ToLower().Replace(" ", "").Contains("hoanthanh");
                                        
            bool isOldStatusCompleted = !string.IsNullOrEmpty(oldStatus) && 
                                        (oldStatus.ToLower().Replace(" ", "").Contains("hoantat") || 
                                         oldStatus.ToLower().Replace(" ", "").Contains("hoanthanh"));

            if (isNewStatusCompleted && !isOldStatusCompleted)
            {
                try 
                {
                    Console.WriteLine($"[ORDERING] Auto-syncing sales for order {maDH}...");
                    var client = _httpClientFactory.CreateClient("CatalogService");
                    var salesUpdates = donHang.ChiTietDonHangs.Select(x => new SalesUpdateDto {
                        MaCTSP = x.MaCTSP,
                        ProductName = x.TenSP_LuuTru,
                        Quantity = x.SoLuong
                    }).ToList();

                    // Gọi Catalog với isFullSync = false để CHỈ CỘNG DỒN thêm vào lượt bán
                    await client.PostAsJsonAsync("api/SanPham/sync-sales-count?isFullSync=false", salesUpdates);

                    // THÊM: Trừ số lượng tồn kho (Inventory decrement)
                    Console.WriteLine($"[ORDERING] Decrementing stock for order {maDH}...");
                    foreach (var item in donHang.ChiTietDonHangs)
                    {
                        try
                        {
                            // Catalog API: PATCH api/ChiTietSanPham/{id}/stock?change={-quantity}
                            var stockResponse = await client.PatchAsync($"api/ChiTietSanPham/{item.MaCTSP}/stock?change={-item.SoLuong}", null);
                            if (!stockResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"[ORDERING] Failed to update stock for variant {item.MaCTSP}: {stockResponse.StatusCode}");
                            }
                        }
                        catch (Exception innerEx)
                        {
                            Console.WriteLine($"[ORDERING] Exception updating stock for variant {item.MaCTSP}: {innerEx.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ORDERING] Failed to auto-sync sales/stock: {ex.Message}");
                }
            }

            return true;
        }

        public async Task<PagedDonHangResult> GetAllDonHangsAsync(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var query = _context.DonHangs.AsQueryable();
            var totalCount = await query.CountAsync();

            var donHangs = await query
                .Include(d => d.ChiTietDonHangs)
                .OrderByDescending(d => d.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedDonHangResult
            {
                TotalCount = totalCount,
                Items = donHangs.Select(MapToDto),
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
                HoTen = donHang.HoTen,
                SoDienThoai = donHang.SoDienThoai,
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
                    Gia_LuuTru = c.Gia_LuuTru,
                    Anh_LuuTru = c.Anh_LuuTru
                }).ToList()
            };
        }

        public async Task<bool> SyncSalesCountAsync()
        {
            try
            {
                // Lấy tất cả đơn hàng (Tránh lỗi nếu TrangThaiDH bị null)
                var allOrders = await _context.DonHangs
                    .Include(d => d.ChiTietDonHangs)
                    .ToListAsync();

                Console.WriteLine($"[DEBUG] Total orders in DB: {allOrders.Count}");
                foreach(var o in allOrders) 
                {
                    Console.WriteLine($"[DEBUG] Order {o.MaDH} has Status: '{o.TrangThaiDH}'");
                }

                // Lọc cực kỳ lỏng lẻo để không bao giờ trượt
                var completedOrders = allOrders
                    .Where(d => !string.IsNullOrEmpty(d.TrangThaiDH) && 
                                (d.TrangThaiDH.ToLower().Replace(" ", "").Contains("hoantat") || 
                                 d.TrangThaiDH.ToLower().Replace(" ", "").Contains("hoanthanh")))
                    .ToList();

                Console.WriteLine($"[ORDERING SYNC] Found {completedOrders.Count} completed orders after AGGRESSIVE filtering.");

                if (!completedOrders.Any())
                {
                    Console.WriteLine("[ORDERING SYNC] No completed orders found. Resetting all sales to 0.");
                    var emptyList = new List<SalesUpdateDto>();
                    var clientReset = _httpClientFactory.CreateClient("CatalogService");
                    await clientReset.PostAsJsonAsync("api/SanPham/sync-sales-count", emptyList);
                    return true;
                }

                // Gộp tổng số lượng bán theo từng MaCTSP/ProductName
                var salesUpdates = completedOrders
                    .SelectMany(d => d.ChiTietDonHangs)
                    .GroupBy(ct => new { ct.MaCTSP, ct.TenSP_LuuTru })
                    .Select(g => new SalesUpdateDto
                    {
                        MaCTSP = g.Key.MaCTSP,
                        ProductName = g.Key.TenSP_LuuTru,
                        Quantity = g.Sum(x => x.SoLuong)
                    })
                    .ToList();

                Console.WriteLine($"[ORDERING SYNC] Sending updates for {salesUpdates.Count} items to Catalog Service.");
                foreach(var item in salesUpdates)
                {
                    Console.WriteLine($"[ORDERING SYNC] -> Sending MaCTSP: {item.MaCTSP} | Qty: {item.Quantity}");
                }

                var client = _httpClientFactory.CreateClient("CatalogService");
                var response = await client.PostAsJsonAsync("api/SanPham/sync-sales-count", salesUpdates);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ORDERING SYNC] Catalog API Error: {response.StatusCode} - {error}");
                    return false;
                }

                Console.WriteLine("[ORDERING SYNC] Sync completed successfully!");
                return true;
            }
            catch (Exception ex)
            {
                var error = $"CRITICAL ERROR: {ex.Message} | StackTrace: {ex.StackTrace}";
                Console.WriteLine($"[ORDERING SYNC] {error}");
                if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
                return false;
            }
        }
    }
}