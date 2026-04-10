using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using OrderService.DTOs;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CartController : ControllerBase
	{
		private readonly ICartService _cartService;

		public CartController(ICartService cartService)
		{
			_cartService = cartService;
		}

		[HttpGet("{customerId}")]
		public async Task<IActionResult> GetCart(int customerId)
		{
			var cart = await _cartService.GetCartByCustomerAsync(customerId);
			return Ok(cart);
		}

		[HttpPost("{customerId}/add")]
		public async Task<IActionResult> AddItem(int customerId, [FromBody] CartItemDTO itemDto)
		{
			await _cartService.AddItemToCartAsync(customerId, itemDto);
			return Ok();
		}

		[HttpDelete("{customerId}/remove/{menuItemId}")]
		public async Task<IActionResult> RemoveItem(int customerId, int menuItemId)
		{
			await _cartService.RemoveItemFromCartAsync(customerId, menuItemId);
			return Ok();
		}

		[HttpDelete("{customerId}/clear")]
		public async Task<IActionResult> ClearCart(int customerId)
		{
			await _cartService.ClearCartAsync(customerId);
			return Ok();
		}
	}
}