using Microsoft.AspNetCore.Mvc;
using BasketService.BasketAPI.Models;
using BasketService.BasketAPI.Services;
using BasketService.BasketAPI.DTOs;

namespace BasketService.BasketAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;
    private readonly ICatalogService _catalogService;
    private readonly IIdentityService _identityService;

    public BasketController(
        IBasketService basketService, 
        ILogger<BasketController> logger,
        ICatalogService catalogService,
        IIdentityService identityService)
    {
        _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    [HttpGet("{userName}")]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
    {
        if (string.IsNullOrEmpty(userName)) return BadRequest("UserName is required.");

        var basket = await _basketService.GetBasketAsync(userName);

        return Ok(basket ?? new ShoppingCart(userName));
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] UpdateCartRequest request)
    {
        if (request?.Cart == null)
            return BadRequest("Invalid request.");

        // Gọi Identity API để verify userName
        var isValidUser = await _identityService.ValidateUserAsync(request.Cart.UserName);
        if (!isValidUser)
        {
            _logger.LogWarning($"User {request.Cart.UserName} is not valid according to IdentityService.");
            return BadRequest("Invalid user.");
        }

        // Gọi Catalog API để cập nhật chính xác giá
        foreach (var item in request.Cart.Items)
        {
            var product = await _catalogService.GetProductAsync(item.ProductId);
            if (product != null)
            {
                item.Price = product.Price;
                item.ProductName = product.Name;
            }
        }

        var updatedCart = await _basketService.UpdateBasketAsync(request.Cart);
        return Ok(updatedCart);
    }

    [HttpDelete("{userName}")]
    public async Task<IActionResult> DeleteBasket(string userName)
    {
        if (string.IsNullOrEmpty(userName)) return BadRequest("UserName is required.");

        await _basketService.DeleteBasketAsync(userName);
        return Ok();
    }
}
