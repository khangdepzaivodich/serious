using BasketService.BasketAPI.Models;

namespace BasketService.BasketAPI.DTOs;

public class UpdateCartRequest
{
    public required ShoppingCart Cart { get; set; }
}

public class DeleteCartRequest
{
    public required string UserName { get; set; }
}
