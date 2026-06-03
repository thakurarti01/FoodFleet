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
            // FindAsync is the most efficient way to look up by primary key — uses EF Core's identity cache
            var restaurant = await _context.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
                throw new Exception("Restaurant not found");

            // Build the Complaint entity from the incoming DTO
            var complaint = new Complaint
            {
                RestaurantId = restaurantId,
                CustomerId = dto.CustomerId,
                OrderId = dto.OrderId,
                ComplaintType = dto.ComplaintType,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,  // Base64 encoded image or null
                Status = "Pending",       // All complaints start as Pending — admin resolves them
                CreatedAt = DateTime.UtcNow
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            // After saving, check if this restaurant has hit the complaint threshold
            var activeCount = await GetActiveComplaintCountAsync(restaurantId);

            // Auto-suspension rule: 5 or more unresolved complaints triggers a 1-month suspension
            // Only suspend if currently Approved — don't double-suspend a Rejected restaurant
            if (activeCount >= 5 && restaurant.ApprovalStatus == "Approved")
            {
                restaurant.ApprovalStatus = "Rejected";
                // AddMonths(1) adds exactly one calendar month to the current UTC time
                restaurant.SuspendedUntil = DateTime.UtcNow.AddMonths(1);
                restaurant.RejectionReason = $"Automatically suspended due to {activeCount} unresolved complaints. Suspension until: {restaurant.SuspendedUntil:yyyy-MM-dd}.";
                await _context.SaveChangesAsync();
                Console.WriteLine($"[ComplaintService] Restaurant {restaurantId} auto-suspended until {restaurant.SuspendedUntil:yyyy-MM-dd} due to {activeCount} complaints.");
            }

            return complaint;
        }

        public async Task<IEnumerable<Complaint>> GetByRestaurantAsync(Guid restaurantId)
        {
            return await _context.Complaints
                // !c.IsDeleted — soft delete filter; deleted complaints are hidden but not removed from DB
                .Where(c => c.RestaurantId == restaurantId && !c.IsDeleted)
                // OrderByDescending shows newest complaints first
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Complaint>> GetAllAsync()
        {
            // Admin view — returns all non-deleted complaints across all restaurants
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

            // Admin sets the resolution status (Resolved or Rejected) and provides a response
            complaint.Status = dto.Status;
            complaint.AdminResponse = dto.AdminResponse;
            complaint.ResolvedAt = DateTime.UtcNow; // Timestamp when admin resolved it

            await _context.SaveChangesAsync();
            return complaint;
        }

        // Counts only Pending (unresolved) complaints — resolved ones don't count toward suspension
        public async Task<int> GetActiveComplaintCountAsync(Guid restaurantId)
        {
            return await _context.Complaints
                .Where(c => c.RestaurantId == restaurantId && c.Status == "Pending" && !c.IsDeleted)
                // CountAsync is more efficient than ToListAsync().Count — generates SQL COUNT(*)
                .CountAsync();
        }
    }
}
