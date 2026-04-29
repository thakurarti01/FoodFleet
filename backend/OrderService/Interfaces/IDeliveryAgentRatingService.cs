using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IDeliveryAgentRatingService
    {
        Task<DeliveryAgentRating> CreateAsync(Guid deliveryAgentId, CreateDeliveryAgentRatingDto dto);
        Task<IEnumerable<DeliveryAgentRating>> GetByAgentAsync(Guid deliveryAgentId);
        Task<DeliveryAgentRating?> GetByOrderAsync(int orderId);
        Task<double> GetAverageRatingAsync(Guid deliveryAgentId);
    }
}
