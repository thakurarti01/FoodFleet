using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Repositories
{
	public class OrderRepository
	{
		private readonly OrderDbContext _context;

		public OrderRepository(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<Order> GetByIdAsync(int orderId)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == orderId);
		}

		public async Task<List<Order>> GetByCustomerAsync(int customerId)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.Where(o => o.CustomerId == customerId)
				.ToListAsync();
		}

		public async Task<Order> AddAsync(Order order)
		{
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
			return order;
		}

		public async Task<bool> UpdateStatusAsync(int orderId, string status)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order == null || order.IsCancelled) return false;

			order.Status = status;
			order.UpdatedAt = System.DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> CancelAsync(int orderId)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order == null || (order.Status != "Placed" && order.Status != "Confirmed")) return false;

			order.IsCancelled = true;
			order.Status = "Cancelled";
			order.UpdatedAt = System.DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}