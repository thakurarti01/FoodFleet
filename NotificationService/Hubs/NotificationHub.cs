using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NotificationService.Hubs
{
	public class NotificationHub : Hub
	{
		/// <summary>
		/// Called when a client connects. Can be used to associate connection with a user.
		/// </summary>
		/// <returns></returns>
		public override async Task OnConnectedAsync()
		{
			var userId = Context.UserIdentifier; // Make sure UserIdentifier is set (JWT or query)
			if (!string.IsNullOrEmpty(userId))
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, userId);
			}
			await base.OnConnectedAsync();
		}

		/// <summary>
		/// Send notification to a specific user by UserId
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task SendNotificationToUser(string userId, string message)
		{
			await Clients.User(userId).SendAsync("ReceiveNotification", message);
		}

		/// <summary>
		/// Optional: Send notification to all connected clients
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public async Task SendNotificationToAll(string message)
		{
			await Clients.All.SendAsync("ReceiveNotification", message);
		}
	}
}