using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrderingService.Ordering.API.DTOs;

namespace OrderingService.Ordering.API.OrderingServices.Interfaces
{
    public interface IDonHangService
    {
        Task<DonHangDto> CreateDonHangAsync(CreateDonHangRequest request);
        Task<DonHangDto?> GetDonHangByIdAsync(Guid maDH);
        Task<IEnumerable<DonHangDto>> GetDonHangsByUserIdAsync(Guid maTK);
        Task<PagedDonHangResult> GetAllDonHangsAsync(int page, int pageSize);
        Task<bool> UpdateDonHangStatusAsync(Guid maDH, string newStatus);
    }
}