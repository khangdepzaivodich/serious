using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data
{
    public static class Seeder
    {
        public static async Task SeedAsync(AppDbContext context, string? jsonFilePath = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Avoid seeding if data already exists
            if (await context.LoaiDanhMucs.AnyAsync()) return;

            // Determine JSON path. Default assumes repository layout:
            // <solution-root>/Services/CatalogService/Data/data.json
            var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "CatalogService", "Data", "data.json");
            var path = jsonFilePath ?? defaultPath;

            if (!File.Exists(path))
            {
                // Try fallback to current directory (useful when running from project output)
                var alt = Path.Combine(AppContext.BaseDirectory ?? string.Empty, "data.json");
                if (File.Exists(alt)) path = alt;
                else throw new FileNotFoundException("Seed data file not found.", path);
            }

            var json = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var seedItems = JsonSerializer.Deserialize<List<SeedLoaiDanhMuc>>(json, options);
            if (seedItems == null || seedItems.Count == 0) return;

            // Map deserialized DTOs to domain models
            var ldmEntities = new List<LoaiDanhMuc>();
            foreach (var ldm in seedItems)
            {
                var ldmEntity = new LoaiDanhMuc
                {
                    MaLDM = ldm.MaLDM,
                    TenLDM = ldm.TenLDM
                };

                if (ldm.DanhMucs != null)
                {
                    foreach (var dm in ldm.DanhMucs)
                    {
                        var dmEntity = new DanhMuc
                        {
                            MaDM = dm.MaDM,
                            MaLDM = dm.MaLDM,
                            TenDM = dm.TenDM
                        };

                        if (dm.SanPhams != null)
                        {
                            foreach (var sp in dm.SanPhams)
                            {
                                var spEntity = new SanPham
                                {
                                    MaSP = sp.MaSP,
                                    MaDM = sp.MaDM,
                                    TenSP = sp.TenSP,
                                    MoTa = sp.MoTa
                                };

                                if (sp.ChiTietSanPhams != null)
                                {
                                    foreach (var ct in sp.ChiTietSanPhams)
                                    {
                                        var ctEntity = new ChiTietSanPham
                                        {
                                            MaCTSP = ct.MaCTSP,
                                            MaSP = ct.MaSP,
                                            Mau = ct.Mau,
                                            KichCo = ct.KichCo,
                                            Gia = ct.Gia,
                                            SoLuong = ct.SoLuong,
                                            Anh = ct.Anh
                                        };

                                        spEntity.ChiTietSanPhams.Add(ctEntity);
                                    }
                                }

                                dmEntity.SanPhams.Add(spEntity);
                            }
                        }

                        ldmEntity.DanhMucs.Add(dmEntity);
                    }
                }

                ldmEntities.Add(ldmEntity);
            }

            // Bulk add and persist
            await context.LoaiDanhMucs.AddRangeAsync(ldmEntities);
            await context.SaveChangesAsync();
        }

        // DTO types matching JSON structure, used only for deserialization
        private class SeedLoaiDanhMuc
        {
            public Guid MaLDM { get; set; }
            public string TenLDM { get; set; } = null!;
            public List<SeedDanhMuc>? DanhMucs { get; set; }
        }

        private class SeedDanhMuc
        {
            public Guid MaDM { get; set; }
            public Guid MaLDM { get; set; }
            public string TenDM { get; set; } = null!;
            public List<SeedSanPham>? SanPhams { get; set; }
        }

        private class SeedSanPham
        {
            public Guid MaSP { get; set; }
            public Guid MaDM { get; set; }
            public string TenSP { get; set; } = null!;
            public string? MoTa { get; set; }
            public List<SeedChiTietSanPham>? ChiTietSanPhams { get; set; }
        }

        private class SeedChiTietSanPham
        {
            public Guid MaCTSP { get; set; }
            public Guid MaSP { get; set; }
            public string Mau { get; set; } = null!;
            public string KichCo { get; set; } = null!;
            public decimal Gia { get; set; }
            public int SoLuong { get; set; }
            public string? Anh { get; set; }
        }
    }
}
