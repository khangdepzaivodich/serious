using CatalogService.DTOs;

namespace CatalogService.CatalogServices.Interfaces
{
    public interface ILoaiDanhMucService
    {
        Task<IEnumerable<LoaiDanhMucDTO>> GetAllAsync();
        Task<LoaiDanhMucDTO?> GetByIdAsync(Guid id);
        Task<LoaiDanhMucDTO> CreateAsync(LoaiDanhMucCreateUpdateDTO dto);
        Task<bool> UpdateAsync(Guid id, LoaiDanhMucCreateUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
