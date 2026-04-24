using System;

namespace OrderingService.Ordering.API.DTOs
{
    public class SalesUpdateDto
    {
        public Guid MaCTSP { get; set; }
        public string? ProductName { get; set; } // Thêm tên để dự phòng
        public int Quantity { get; set; }
    }
}
