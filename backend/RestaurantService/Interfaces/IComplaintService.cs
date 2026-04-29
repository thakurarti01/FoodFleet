using RestaurantService.DTOs;
using RestaurantService.Models;

namespace RestaurantService.Interfaces
{
    public interface IComplaintService
    {
        Task<Complaint> CreateAsync(Guid restaurantId, CreateComplaintDto dto);
        Task<IEnumerable<Complaint>> GetByRestaurantAsync(Guid restaurantId);
        Task<IEnumerable<Complaint>> GetAllAsync();
        Task<Complaint> ResolveAsync(int complaintId, ResolveComplaintDto dto);
        Task<int> GetActiveComplaintCountAsync(Guid restaurantId);
    }
}
