using System.Threading.Tasks;

namespace OrderService.Interfaces
{
	public interface INotificationService
	{
		Task SendOrderNotificationToOwnerAsync(int restaurantId, int orderId);
		Task SendOrderStatusUpdateToCustomerAsync(int customerId, int orderId, string status);
	}
}