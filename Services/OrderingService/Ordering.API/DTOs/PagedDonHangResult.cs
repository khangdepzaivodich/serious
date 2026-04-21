using System.Collections.Generic;

namespace OrderingService.Ordering.API.DTOs
{
    public class PagedDonHangResult
    {
        public IEnumerable<DonHangDto> Items { get; set; } = new List<DonHangDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
