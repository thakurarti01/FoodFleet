using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Services
{
	public class CartService : ICartService
	{
		private readonly OrderDbContext _context;

		public CartService(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<Cart> GetCartByCustomerAsync(int customerId)
		{
			var cart = await _context.Carts
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.CustomerId == customerId);

			if (cart == null)
			{
				cart = new Cart { CustomerId = customerId };
				_context.Carts.Add(cart);
				await _context.SaveChangesAsync();
			}

			return cart;
		}

		public async Task AddItemToCartAsync(int customerId, CartItemDTO itemDto)
		{
			var cart = await GetCartByCustomerAsync(customerId);

			var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == itemDto.MenuItemId);
			if (existingItem != null)
			{
				existingItem.Quantity += itemDto.Quantity;
				existingItem.Customizations = itemDto.Customizations;
			}
			else
			{
				cart.Items.Add(new CartItem
				{
					MenuItemId = itemDto.MenuItemId,
					Quantity = itemDto.Quantity,
					Customizations = itemDto.Customizations
				});
			}

			await _context.SaveChangesAsync();
		}

		public async Task RemoveItemFromCartAsync(int customerId, int menuItemId)
		{
			var cart = await GetCartByCustomerAsync(customerId);
			var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
			if (item != null)
			{
				cart.Items.Remove(item);
				await _context.SaveChangesAsync();
			}
		}

		public async Task ClearCartAsync(int customerId)
		{
			var cart = await GetCartByCustomerAsync(customerId);
			cart.Items.Clear();
			await _context.SaveChangesAsync();
		}
	}
}