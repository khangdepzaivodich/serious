using CatalogService.DTOs;

namespace CatalogService.CatalogServices.Interfaces
{
    public interface ISanPhamService
    {
        Task<(IEnumerable<SanPhamDTO> Data, int TotalCount)> GetPagedSanPhamsAsync(SanPhamPaginationDTO paginationDto);
        Task<SanPhamDTO> GetSanPhamByIdAsync(Guid id);
        Task<SanPhamDTO> CreateSanPhamAsync(SanPhamCreateDTO createDto);
        Task<bool> UpdateSanPhamAsync(Guid id, SanPhamCreateDTO updateDto);
        Task<bool> DeleteSanPhamAsync(Guid id);
    }
}