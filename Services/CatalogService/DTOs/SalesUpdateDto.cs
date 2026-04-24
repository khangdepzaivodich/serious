using System;

namespace CatalogService.DTOs
{
    public class SalesUpdateDto
    {
        public Guid MaCTSP { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
