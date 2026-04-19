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

    public BasketController(IBasketService basketService, ILogger<BasketController> logger)
    {
        _basketService = basketService ?? throw new ArgumentNullException(nameof(basketService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
