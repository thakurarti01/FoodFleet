using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(PlaceOrderDto dto);
        Task<Order?> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Order>> GetByRestaurantAsync(Guid restaurantId);
        Task<IEnumerable<Order>> GetByAgentAsync(Guid agentId);
        Task<IEnumerable<Order>> GetUnassignedAsync();
        Task<IEnumerable<Order>> GetActiveDeliveriesAsync();
        Task<bool> UpdateStatusAsync(int orderId, string status);
        Task<bool> CancelAsync(int orderId, string reason);
        Task<bool> AssignAgentAsync(int orderId, Guid agentId);
        Task<bool> UpdateDeliveryStatusAsync(int orderId, string deliveryStatus);
        Task<decimal> GetRestaurantEarningsAsync(Guid restaurantId);
        Task<decimal> GetAgentEarningsAsync(Guid agentId);
    }
}
