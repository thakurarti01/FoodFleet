using DeliveryService.Models;
using DeliveryService.DTOs;

namespace DeliveryService.Interfaces
{
	public interface IDeliveryService
	{
		Task<Delivery> AssignDeliveryAsync(DeliveryDTO deliveryDTO);
		Task<bool> RespondToDeliveryAsync(int deliveryId, bool isAccepted);
		Task<bool> UpdateStatusAsync(int deliveryId, string status);
		Task<Delivery> GetDeliveryByIdAsync(int deliveryId);
		Task<object> TrackDeliveryAsync(int deliveryId);
	}
}