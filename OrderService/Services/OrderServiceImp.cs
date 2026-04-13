using Microsoft.EntityFrameworkCore;
using OrderService.Services;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Interfaces;

namespace OrderService.Services
{
	public class OrderServiceImp : IOrderService
	{
		private readonly OrderDbContext _context;

		public OrderServiceImp(OrderDbContext context)
		{
			_context = context;
		}

		public async Task<Order> PlaceOrderAsync(OrderDTO orderDTO)
		{
			var order = new Order
			{
				CustomerId = orderDTO.CustomerId,
				RestaurantId = orderDTO.RestaurantId,
				DeliveryAddressId = orderDTO.DeliveryAddressId,
				DeliveryFee = orderDTO.DeliveryFee,
				Discounts = orderDTO.Discounts,
				Taxes = orderDTO.Taxes,
				Status = "Placed",
				PlacedAt = DateTime.UtcNow,
				Items = orderDTO.Items.Select(i => new OrderItem
				{
					MenuItemId = i.MenuItemId,
					Quantity = i.Quantity,
					Customizations = i.Customizations,
					Price = i.Price
				}).ToList()
			};

			order.TotalPrice = order.Items.Sum(i => i.Price * i.Quantity) + order.DeliveryFee + order.Taxes - order.Discounts;

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			return order;
		}

		public async Task<Order> GetOrderByIdAsync(int orderId)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == orderId)
				?? throw new Exception("Order not found");
		}

		public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.Where(o => o.CustomerId == customerId)
				.ToListAsync();
		}

		public async Task<bool> CancelOrderAsync(int orderId)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order == null || order.Status != "Placed" && order.Status != "Confirmed")
				return false;

			order.IsCancelled = true;
			order.Status = "Cancelled";
			order.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UpdateStatusAsync(int orderId, string newStatus)
		{
			var order = await _context.Orders.FindAsync(orderId);
			if (order == null || order.IsCancelled)
				return false;

			order.Status = newStatus;
			order.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
			return true;
		}

		public OrderDTO MapToDTO(Order order)
		{
			return new OrderDTO
			{
				CustomerId = order.CustomerId,
				RestaurantId = order.RestaurantId,
				DeliveryAddressId = order.DeliveryAddressId,
				DeliveryFee = order.DeliveryFee,
				Discounts = order.Discounts,
				Taxes = order.Taxes,
				Items = order.Items.Select(i => new DTOs.OrderItemDTO
				{
					MenuItemId = i.MenuItemId,
					Quantity = i.Quantity,
					Customizations = i.Customizations,
					Price = i.Price
				}).ToList(),
				//DeliveryFee = order.DeliveryFee,
				//Discounts = order.Discounts,
				//Taxes = order.Taxes,
				TotalPrice = order.TotalPrice,
				Status = order.Status,
				IsCancelled = order.IsCancelled,
				PlacedAt = order.PlacedAt,
				UpdatedAt = order.UpdatedAt
			};
		}
	}
}