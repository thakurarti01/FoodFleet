using RestaurantService.DTOs;
using RestaurantService.Models;

namespace RestaurantService.Interfaces
{
    public interface IRestaurantService
    {
        Task<IEnumerable<Restaurant>> GetAllAsync(bool adminView = false);
        Task<Restaurant?> GetByIdAsync(Guid id);
        Task<Guid> CreateAsync(CreateRestaurantDto dto, Guid ownerId);
        Task<bool> UpdateAsync(Guid id, CreateRestaurantDto dto, Guid requesterId);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<bool> RejectAsync(Guid id, string reason);
        Task<bool> ToggleOpenAsync(Guid id, bool isOpen);
    }

    public interface IMenuService
    {
        Task<IEnumerable<MenuItem>> GetByRestaurantAsync(Guid restaurantId);
        Task<MenuItem?> GetByIdAsync(int id);
        Task<MenuItem> CreateAsync(Guid restaurantId, CreateMenuItemDto dto);
        Task<bool> UpdateAsync(int id, UpdateMenuItemDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<MenuCategory>> GetCategoriesAsync();
        Task<MenuCategory> CreateCategoryAsync(string name, string description);
    }

    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetByRestaurantAsync(Guid restaurantId);
        Task<Review?> GetByOrderAsync(int orderId);
        Task<Review> CreateAsync(Guid restaurantId, CreateReviewDto dto);
        Task<bool> UpdateAsync(int reviewId, UpdateReviewDto dto, Guid customerId);
        Task<bool> DeleteAsync(int reviewId);
        Task<bool> AddOwnerResponseAsync(int reviewId, OwnerResponseDto dto);
        Task<double> GetAverageRatingAsync(Guid restaurantId);
    }
}
