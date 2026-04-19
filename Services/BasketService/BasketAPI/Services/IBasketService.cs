using BasketService.BasketAPI.Models;

namespace BasketService.BasketAPI.Services;

public interface IBasketService
{
    Task<ShoppingCart?> GetBasketAsync(string userName);
    Task<ShoppingCart?> UpdateBasketAsync(ShoppingCart basket);
    Task DeleteBasketAsync(string userName);
}
