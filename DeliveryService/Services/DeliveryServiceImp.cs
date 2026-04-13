using DeliveryService.Data;
using DeliveryService.DTOs;
using DeliveryService.Interfaces;
using DeliveryService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Services
{
	public class DeliveryServiceImp : IDeliveryService
	{
		private readonly DeliveryDbContext _context;
		private readonly RabbitMQPublisher _publisher;

		public DeliveryServiceImp(DeliveryDbContext context, RabbitMQPublisher publisher)
		{
			_context = context;
			_publisher = publisher;
		}

		// Assigning nearest agent
		public async Task<Delivery> AssignDeliveryAsync(DeliveryDTO dto)
		{
			var agents = await _context.DeliveryAgents
				.Where(a => a.IsAvailable)
				.ToListAsync();

			if (!agents.Any()) throw new InvalidOperationException("No delivery agents are currently available.");

			var nearestAgent = agents
				.OrderBy(a => GetDistance(dto.PickupLatitude, dto.PickupLongitude,
										  a.CurrentLatitude, a.CurrentLongitude))
				.First();

			nearestAgent.IsAvailable = false;

			var delivery = new Delivery
			{
				OrderId = dto.OrderId,
				AgentId = nearestAgent.Id,
				PickupLocation = dto.PickupLocation,
				DeliveryLocation = dto.DeliveryLocation,
				Status = "Assigned",
				EstimatedTime = (int)Math.Round(GetDistance(dto.PickupLatitude, dto.PickupLongitude,
				nearestAgent.CurrentLatitude, nearestAgent.CurrentLongitude) * 2 + 10), // ~2 min/km + 10 min base
				CustomerEmail = dto.CustomerEmail,
				CustomerName = dto.CustomerName,
				ItemNames = dto.ItemNames
			};

			_context.Deliveries.Add(delivery);
			await _context.SaveChangesAsync();

			return delivery;
		}

		// Accept / Reject
		public async Task<bool> RespondToDeliveryAsync(int deliveryId, bool isAccepted)
		{
			var delivery = await _context.Deliveries.FindAsync(deliveryId);
			if (delivery == null) return false;

			if (isAccepted)
			{
				delivery.Status = "Accepted";
			}
			else
			{
				delivery.Status = "Rejected";

				var agent = await _context.DeliveryAgents.FindAsync(delivery.AgentId);
				if (agent != null)
					agent.IsAvailable = true;
			}

			await _context.SaveChangesAsync();
			return true;
		}

		//  Update Status
		public async Task<bool> UpdateStatusAsync(int deliveryId, string status)
		{
			var delivery = await _context.Deliveries.FindAsync(deliveryId);
			if (delivery == null) return false;

			delivery.Status = status;

			if (status == "Delivered")
			{
				delivery.ActualDeliveryTime = DateTime.UtcNow;

				var agent = await _context.DeliveryAgents.FindAsync(delivery.AgentId);
				if (agent != null)
					agent.IsAvailable = true;

				// Publish delivered event if customer info is available
				if (!string.IsNullOrEmpty(delivery.CustomerEmail))
				{
					var itemNames = string.IsNullOrEmpty(delivery.ItemNames)
						? new List<string>()
						: delivery.ItemNames.Split(',').ToList();

					_publisher.PublishOrderDelivered(delivery.OrderId, delivery.CustomerEmail, delivery.CustomerName ?? "Customer", itemNames);
				}
			}

			await _context.SaveChangesAsync();
			return true;
		}

		//  Get Delivery
		public async Task<Delivery> GetDeliveryByIdAsync(int deliveryId)
		{
			return await _context.Deliveries
				.Include(d => d.Agent)
				.FirstOrDefaultAsync(d => d.Id == deliveryId);
		}

		//  Track (for customer)
		public async Task<object> TrackDeliveryAsync(int deliveryId)
		{
			var delivery = await _context.Deliveries
				.Include(d => d.Agent)
				.FirstOrDefaultAsync(d => d.Id == deliveryId);

			if (delivery == null) return null;

			return new
			{
				delivery.Id,
				delivery.Status,
				delivery.Agent.CurrentLatitude,
				delivery.Agent.CurrentLongitude
			};
		}

		//  Distance Logic
		private double GetDistance(double lat1, double lon1, double lat2, double lon2)
		{
			return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lon1 - lon2, 2));
		}
	}
}