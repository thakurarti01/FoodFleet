using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Interfaces
{
    public interface IDeliveryAgentComplaintService
    {
        Task<DeliveryAgentComplaint> CreateAsync(Guid deliveryAgentId, CreateDeliveryAgentComplaintDto dto);
        Task<IEnumerable<DeliveryAgentComplaint>> GetByAgentAsync(Guid deliveryAgentId);
        Task<IEnumerable<DeliveryAgentComplaint>> GetAllAsync();
        Task<DeliveryAgentComplaint> ResolveAsync(int complaintId, ResolveDeliveryAgentComplaintDto dto);
        Task<int> GetActiveComplaintCountAsync(Guid deliveryAgentId);
    }
}
