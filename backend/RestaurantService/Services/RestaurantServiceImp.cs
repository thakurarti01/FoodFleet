using Microsoft.EntityFrameworkCore;
using RestaurantService.Data;
using RestaurantService.DTOs;
using RestaurantService.Exceptions;
using RestaurantService.Interfaces;
using RestaurantService.Models;

namespace RestaurantService.Services
{
    /// <summary>
    /// Manages restaurant CRUD, admin approval/rejection workflow,
    /// and open/closed toggle. Only Approved restaurants are visible to the public.
    /// </summary>
    public class RestaurantServiceImp : IRestaurantService
    {
        private readonly RestaurantDbContext _context;

        public RestaurantServiceImp(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Restaurant>> GetAllAsync(bool adminView = false)
        {
            try
            {
                var query = _context.Restaurants.AsQueryable();
                if (!adminView)
                    query = query.Where(r => r.ApprovalStatus == "Approved");
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve restaurants: {ex.Message}", ex);
            }
        }

        public async Task<Restaurant?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Restaurants
                    .Include(r => r.MenuItems)
                        .ThenInclude(m => m.Category)
                    .Include(r => r.Reviews.Where(rv => !rv.IsDeleted))
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve restaurant: {ex.Message}", ex);
            }
        }

        public async Task<Guid> CreateAsync(CreateRestaurantDto dto, Guid ownerId)
        {
            try
            {
                var restaurant = new Restaurant
                {
                    OwnerId = ownerId,
                    Name = dto.Name,
                    Description = dto.Description,
                    Address = dto.Address,
                    CuisineTypes = dto.CuisineTypes,
                    LogoUrl = dto.LogoUrl,
                    ApprovalStatus = "Pending",
                    IsOpen = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Restaurants.Add(restaurant);
                await _context.SaveChangesAsync();
                return restaurant.Id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create restaurant: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAsync(Guid id, CreateRestaurantDto dto, Guid requesterId)
        {
            try
            {
                var r = await _context.Restaurants.FindAsync(id)
                    ?? throw new RestaurantNotFoundException(id);

                r.Name = dto.Name;
                r.Description = dto.Description;
                r.Address = dto.Address;
                r.CuisineTypes = dto.CuisineTypes;
                r.LogoUrl = dto.LogoUrl;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (RestaurantNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update restaurant: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var r = await _context.Restaurants.FindAsync(id)
                    ?? throw new RestaurantNotFoundException(id);

                _context.Restaurants.Remove(r);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (RestaurantNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete restaurant: {ex.Message}", ex);
            }
        }

        public async Task<bool> ApproveAsync(Guid id)
        {
            try
            {
                var r = await _context.Restaurants.FindAsync(id)
                    ?? throw new RestaurantNotFoundException(id);

                r.ApprovalStatus = "Approved";
                r.RejectionReason = null;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (RestaurantNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to approve restaurant: {ex.Message}", ex);
            }
        }

        public async Task<bool> RejectAsync(Guid id, string reason)
        {
            try
            {
                var r = await _context.Restaurants.FindAsync(id)
                    ?? throw new RestaurantNotFoundException(id);

                r.ApprovalStatus = "Rejected";
                r.RejectionReason = reason;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (RestaurantNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reject restaurant: {ex.Message}", ex);
            }
        }

        public async Task<bool> ToggleOpenAsync(Guid id, bool isOpen)
        {
            try
            {
                var r = await _context.Restaurants.FindAsync(id)
                    ?? throw new RestaurantNotFoundException(id);

                if (isOpen && r.ApprovalStatus != "Approved")
                    throw new RestaurantNotApprovedException(r.Name);

                r.IsOpen = isOpen;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (RestaurantNotFoundException) { throw; }
            catch (RestaurantNotApprovedException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to toggle restaurant status: {ex.Message}", ex);
            }
        }
    }
}
