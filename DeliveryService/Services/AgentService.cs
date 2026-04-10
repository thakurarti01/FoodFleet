using DeliveryService.Data;
using DeliveryService.DTOs;
using DeliveryService.Interfaces;
using DeliveryService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services
{
	public class AgentService : IAgentService
	{
		private readonly DeliveryDbContext _context;

		public AgentService(DeliveryDbContext context)
		{
			_context = context;
		}

		public async Task<DeliveryAgent> RegisterAgentAsync(AgentDTO dto)
		{
			var agent = new DeliveryAgent
			{
				Name = dto.Name,
				Phone = dto.Phone,
				VehicleType = dto.VehicleType,
				IsAvailable = true
			};

			_context.DeliveryAgents.Add(agent);
			await _context.SaveChangesAsync();

			return agent;
		}

		public async Task<bool> UpdateAvailabilityAsync(int agentId, bool isAvailable)
		{
			var agent = await _context.DeliveryAgents.FindAsync(agentId);
			if (agent == null) return false;

			agent.IsAvailable = isAvailable;
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<bool> UpdateLocationAsync(int agentId, LocationDTO dto)
		{
			var agent = await _context.DeliveryAgents.FindAsync(agentId);
			if (agent == null) return false;

			agent.CurrentLatitude = dto.Latitude;
			agent.CurrentLongitude = dto.Longitude;

			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<Delivery>> GetAgentHistoryAsync(int agentId)
		{
			return await _context.Deliveries
				.Where(d => d.AgentId == agentId)
				.ToListAsync();
		}
	}
}