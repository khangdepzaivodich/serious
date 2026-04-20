using CatalogService.DTOs;

namespace CatalogService.CatalogServices.Interfaces
{
    public interface ISanPhamService
    {
        Task<IEnumerable<SanPhamDTO>> GetAllSanPhamsAsync();
        Task<SanPhamDTO> GetSanPhamByIdAsync(Guid id);
        Task<SanPhamDTO> CreateSanPhamAsync(SanPhamCreateDTO createDto);
        Task<bool> UpdateSanPhamAsync(Guid id, SanPhamCreateDTO updateDto);
        Task<bool> DeleteSanPhamAsync(Guid id);
    }
}
