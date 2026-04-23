namespace CatalogService.DTOs
{
    public class SanPhamPaginationDTO
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public Guid? MaDM { get; set; } 
        public Guid? MaLDM { get; set; }
        public string? Keyword { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
