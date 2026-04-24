namespace CatalogService.DTOs
{
    public class SanPhamPaginationDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? MaDM { get; set; } 
        public Guid? MaLDM { get; set; }
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; } // "best-seller", "price-asc", "price-desc", "newest"
    }
}
