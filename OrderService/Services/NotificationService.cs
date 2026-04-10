using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using OrderService.Hubs;
using OrderService.Interfaces;

namespace OrderService.Services
{
	public class NotificationService : INotificationService
	{
		private readonly IHubContext<OrderHub> _hub;

		public NotificationService(IHubContext<OrderHub> hub)
		{
			_hub = hub;
		}

		public async Task SendOrderNotificationToOwnerAsync(int restaurantId, int orderId)
		{
			await _hub.Clients.Group($"restaurant-{restaurantId}")
				.SendAsync("NewOrder", orderId);
		}

		public async Task SendOrderStatusUpdateToCustomerAsync(int customerId, int orderId, string status)
		{
			await _hub.Clients.User(customerId.ToString())
				.SendAsync("OrderStatusChanged", new { orderId, status });
		}
	}
}