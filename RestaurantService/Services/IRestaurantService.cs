using RestaurantService.DTO;
using RestaurantService.Models;

namespace RestaurantService.Services
{
	public interface IRestaurantService
	{
		Task<IEnumerable<Restaurant>> GetAllAsync();
		Task<Restaurant?> GetByIdAsync(Guid id);
		Task<Guid> CreateAsync(CreateRestaurantDto dto, Guid ownerId);
		Task<bool> UpdateAsync(Guid id, CreateRestaurantDto dto);
		Task<bool> DeleteAsync(Guid id);
	}
}
