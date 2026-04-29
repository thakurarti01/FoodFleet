using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Interfaces;
using OrderService.Models;

namespace OrderService.Services
{
    /// <summary>
    /// Manages ratings for delivery agents.
    /// One rating per customer per order.
    /// </summary>
    public class DeliveryAgentRatingServiceImp : IDeliveryAgentRatingService
    {
        private readonly OrderDbContext _context;

        public DeliveryAgentRatingServiceImp(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<DeliveryAgentRating> CreateAsync(Guid deliveryAgentId, CreateDeliveryAgentRatingDto dto)
        {
            // Check if rating already exists for this order and customer
            var existing = await _context.DeliveryAgentRatings
                .FirstOrDefaultAsync(r => r.OrderId == dto.OrderId && r.CustomerId == dto.CustomerId);

            if (existing != null)
                throw new InvalidOperationException("You have already rated this delivery");

            var order = await _context.Orders.FindAsync(dto.OrderId);
            if (order == null)
                throw new Exception("Order not found");

            if (order.DeliveryAgentId != deliveryAgentId)
                throw new Exception("This order was not delivered by the specified agent");

            var rating = new DeliveryAgentRating
            {
                DeliveryAgentId = deliveryAgentId,
                CustomerId = dto.CustomerId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryAgentRatings.Add(rating);
            await _context.SaveChangesAsync();

            return rating;
        }

        public async Task<IEnumerable<DeliveryAgentRating>> GetByAgentAsync(Guid deliveryAgentId)
        {
            return await _context.DeliveryAgentRatings
                .Where(r => r.DeliveryAgentId == deliveryAgentId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<DeliveryAgentRating?> GetByOrderAsync(int orderId)
        {
            return await _context.DeliveryAgentRatings
                .FirstOrDefaultAsync(r => r.OrderId == orderId && !r.IsDeleted);
        }

        public async Task<double> GetAverageRatingAsync(Guid deliveryAgentId)
        {
            var ratings = await _context.DeliveryAgentRatings
                .Where(r => r.DeliveryAgentId == deliveryAgentId && !r.IsDeleted)
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }
    }
}
