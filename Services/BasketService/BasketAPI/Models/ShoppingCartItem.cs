namespace BasketService.BasketAPI.Models;

public class ShoppingCartItem
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
