using CatalogService.DTOs;

namespace CatalogService.CatalogServices.Interfaces
{
    public interface IDanhMucService
    {
        Task<IEnumerable<DanhMucDTO>> GetAllAsync();
        Task<IEnumerable<DanhMucDTO>> GetByLoaiDanhMucIdAsync(Guid maLDM);
        Task<DanhMucDTO?> GetByIdAsync(Guid id);
        Task<DanhMucDTO> CreateAsync(DanhMucCreateUpdateDTO dto);
        Task<bool> UpdateAsync(Guid id, DanhMucCreateUpdateDTO dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
