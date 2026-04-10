using DeliveryService.Models;
using DeliveryService.DTOs;

namespace DeliveryService.Interfaces
{
	public interface IAgentService
	{
		Task<DeliveryAgent> RegisterAgentAsync(AgentDTO agentDTO);
		Task<bool> UpdateAvailabilityAsync(int agentId, bool isAvailable);
		Task<bool> UpdateLocationAsync(int agentId, LocationDTO locationDTO);
		Task<IEnumerable<Delivery>> GetAgentHistoryAsync(int agentId);
	}
}