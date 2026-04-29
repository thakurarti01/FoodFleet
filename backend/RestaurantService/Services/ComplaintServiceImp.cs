using Microsoft.EntityFrameworkCore;
using RestaurantService.Data;
using RestaurantService.DTOs;
using RestaurantService.Interfaces;
using RestaurantService.Models;

namespace RestaurantService.Services
{
    /// <summary>
    /// Manages complaints against restaurants.
    /// Tracks complaint count and auto-revokes restaurant after 5 active complaints.
    /// </summary>
    public class ComplaintServiceImp : IComplaintService
    {
        private readonly RestaurantDbContext _context;

        public ComplaintServiceImp(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateAsync(Guid restaurantId, CreateComplaintDto dto)
        {
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
                throw new Exception("Restaurant not found");

            var complaint = new Complaint
            {
                RestaurantId = restaurantId,
                CustomerId = dto.CustomerId,
                OrderId = dto.OrderId,
                ComplaintType = dto.ComplaintType,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // Check if restaurant should be revoked (5 or more active complaints)
            var activeCount = await GetActiveComplaintCountAsync(restaurantId);
            if (activeCount >= 5 && restaurant.ApprovalStatus == "Approved")
            {
                restaurant.ApprovalStatus = "Rejected";
                restaurant.RejectionReason = $"Automatically revoked due to {activeCount} unresolved complaints. Suspension period: 1 month.";
                await _context.SaveChangesAsync();
                Console.WriteLine($"[ComplaintService] Restaurant {restaurantId} auto-revoked due to {activeCount} complaints.");
            }

            return complaint;
        }

        public async Task<IEnumerable<Complaint>> GetByRestaurantAsync(Guid restaurantId)
        {
            return await _context.Complaints
                .Where(c => c.RestaurantId == restaurantId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Complaint>> GetAllAsync()
        {
            return await _context.Complaints
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Complaint> ResolveAsync(int complaintId, ResolveComplaintDto dto)
        {
            var complaint = await _context.Complaints.FindAsync(complaintId);
            if (complaint == null)
                throw new Exception("Complaint not found");

            complaint.Status = dto.Status;
            complaint.AdminResponse = dto.AdminResponse;
            complaint.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return complaint;
        }

        public async Task<int> GetActiveComplaintCountAsync(Guid restaurantId)
        {
            return await _context.Complaints
                .Where(c => c.RestaurantId == restaurantId && c.Status == "Pending" && !c.IsDeleted)
                .CountAsync();
        }
    }
}
