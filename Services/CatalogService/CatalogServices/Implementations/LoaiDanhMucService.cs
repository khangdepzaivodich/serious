using CatalogService.CatalogServices.Interfaces;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.CatalogServices.Implementations
{
    public class LoaiDanhMucService : ILoaiDanhMucService
    {
        private readonly AppDbContext _context;

        public LoaiDanhMucService(AppDbContext context)
        {
            _context = context;
        }

        // GET BY ID
        public async Task<LoaiDanhMucDTO?> GetByIdAsync(Guid id)
        {
            var ldm = await _context.LoaiDanhMucs
                .Include(x => x.DanhMucs)
                .FirstOrDefaultAsync(x => x.MaLDM == id);

            if (ldm == null) return null;

            return new LoaiDanhMucDTO
            {
                MaLDM = ldm.MaLDM,
                TenLDM = ldm.TenLDM,
                DanhMucs = ldm.DanhMucs.Select(d => new DanhMucDTO
                {
                    MaDM = d.MaDM,
                    MaLDM = d.MaLDM,
                    TenDM = d.TenDM
                }).ToList()
            };
        }

        // CREATE
        public async Task<LoaiDanhMucDTO> CreateAsync(LoaiDanhMucCreateUpdateDTO dto)
        {
            var entity = new LoaiDanhMuc
            {
                MaLDM = Guid.NewGuid(),
                TenLDM = dto.TenLDM
            };

            _context.LoaiDanhMucs.Add(entity);
            await _context.SaveChangesAsync();

            return new LoaiDanhMucDTO
            {
                MaLDM = entity.MaLDM,
                TenLDM = entity.TenLDM
            };
        }

        // UPDATE
        public async Task<bool> UpdateAsync(Guid id, LoaiDanhMucCreateUpdateDTO dto)
        {
            var entity = await _context.LoaiDanhMucs.FindAsync(id);
            if (entity == null) return false;

            entity.TenLDM = dto.TenLDM;

            await _context.SaveChangesAsync();
            return true;
        }

        // DELETE
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.LoaiDanhMucs.FindAsync(id);
            if (entity == null) return false;

            _context.LoaiDanhMucs.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<LoaiDanhMucDTO>> GetAllAsync()
        {
            return await _context.LoaiDanhMucs
                .Select(ldm => new LoaiDanhMucDTO
                {
                    MaLDM = ldm.MaLDM,
                    TenLDM = ldm.TenLDM,
                    DanhMucs = ldm.DanhMucs.Select(d => new DanhMucDTO
                    {
                        MaDM = d.MaDM,
                        MaLDM = d.MaLDM,
                        TenDM = d.TenDM
                    }).ToList()
                })
                .ToListAsync();
        }
    }
}