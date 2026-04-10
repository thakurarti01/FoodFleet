using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
	public interface ICartService
	{
		Task<Cart> GetCartByCustomerAsync(int customerId);
		Task AddItemToCartAsync(int customerId, CartItemDTO itemDto);
		Task RemoveItemFromCartAsync(int customerId, int menuItemId);
		Task ClearCartAsync(int customerId);
	}
}