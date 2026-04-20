using CatalogService.DTOs;

namespace CatalogService.CatalogServices.Interfaces
{
    public interface IChiTietSanPhamService
    {
        Task<IEnumerable<ChiTietSanPhamDTO>> GetBySanPhamIdAsync(Guid maSP);
        Task<ChiTietSanPhamDTO?> GetByIdAsync(Guid id);
        Task<ChiTietSanPhamDTO> CreateAsync(ChiTietSanPhamCreateUpdateDTO dto);
        Task<bool> UpdateAsync(Guid id, ChiTietSanPhamCreateUpdateDTO dto);
        Task<bool> UpdateStockAsync(Guid id, int quantityChange); 
        Task<bool> DeleteAsync(Guid id);
    }
}
