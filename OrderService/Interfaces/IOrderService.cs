using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
	public interface IOrderService
	{
		Task<Order> PlaceOrderAsync(OrderDTO orderDTO);
		Task<Order> GetOrderByIdAsync(int orderId);
		Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
		Task<bool> CancelOrderAsync(int orderId);
		Task<bool> UpdateStatusAsync(int orderId, string newStatus);

		// Mapping
		OrderDTO MapToDTO(Order order);
	}
}