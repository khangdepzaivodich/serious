using System.Text.Json;
using StackExchange.Redis;
using BasketService.BasketAPI.Models;

namespace BasketService.BasketAPI.Services;

public class BasketRedisService : IBasketService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;

    public BasketRedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }

    public async Task<ShoppingCart?> GetBasketAsync(string userName)
    {
        var basket = await _database.StringGetAsync(userName);
        if (basket.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<ShoppingCart>(basket.ToString());
    }

    public async Task<ShoppingCart?> UpdateBasketAsync(ShoppingCart basket)
    {
        var basketJson = JsonSerializer.Serialize(basket);
        await _database.StringSetAsync(basket.UserName, basketJson, TimeSpan.FromDays(7)); // Giỏ hàng lưu trữ 7 ngày
        return await GetBasketAsync(basket.UserName);
    }

    public async Task DeleteBasketAsync(string userName)
    {
        await _database.KeyDeleteAsync(userName);
    }
}
