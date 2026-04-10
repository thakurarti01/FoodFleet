using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace OrderService.Hubs
{
	public class OrderHub : Hub
	{
		// Called when a restaurant owner connects
		// Adds a restaurant owner to a group for their restaurant
		public async Task JoinRestaurantGroup(int restaurantId)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, $"restaurant-{restaurantId}");
		}

		// Called when a customer connects
		// Adds a customer to a group based on their user identifier when they connect
		public override async Task OnConnectedAsync()
		{
			var userId = Context.UserIdentifier; // Ensure your authentication sets UserIdentifier
			if (!string.IsNullOrEmpty(userId))
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, $"customer-{userId}");
			}

			await base.OnConnectedAsync();
		}

		// Called when a client disconnects
		public override async Task OnDisconnectedAsync(System.Exception exception)
		{
			// Remove from groups automatically
			await base.OnDisconnectedAsync(exception);
		}
	}
}