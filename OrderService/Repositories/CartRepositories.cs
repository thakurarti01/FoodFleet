using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories
{
	public class CartRepository
	{
		private readonly OrderDbContext _context;

		public CartRepository(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<Cart> GetByCustomerAsync(int customerId)
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

		public async Task AddItemAsync(CartItem item)
		{
			_context.CartItems.Add(item);
			await _context.SaveChangesAsync();
		}

		public async Task RemoveItemAsync(CartItem item)
		{
			_context.CartItems.Remove(item);
			await _context.SaveChangesAsync();
		}

		public async Task ClearCartAsync(Cart cart)
		{
			_context.CartItems.RemoveRange(cart.Items);
			await _context.SaveChangesAsync();
		}
	}
}