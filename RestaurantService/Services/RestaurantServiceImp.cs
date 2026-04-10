using RestaurantService.DTO;
using RestaurantService.Models;
using RestaurantService.Repositories;

namespace RestaurantService.Services
{
	public class RestaurantServiceImp : IRestaurantService
	{
		private readonly IRestaurantRepository _repo;

		public RestaurantServiceImp(IRestaurantRepository repo)
		{
			_repo = repo;
		}

		public async Task<IEnumerable<Restaurant>> GetAllAsync()
		{
			return await _repo.GetAllAsync();
		}

		public async Task<Restaurant?> GetByIdAsync(Guid id)
		{
			return await _repo.GetByIdAsync(id);
		}

		public async Task<Guid> CreateAsync(CreateRestaurantDto dto, Guid ownerId)
		{
			var restaurant = new Restaurant
			{
				OwnerId = ownerId,
				Name = dto.Name,
				Description = dto.Description,
				Address = dto.Address,
				CuisineTypes = dto.CuisineTypes,
				LogoUrl = dto.LogoUrl,
				OperatingHours = dto.OperatingHours,
				MinimumOrderAmount = dto.MinimumOrderAmount,
				EstimatedDeliveryMinutes = dto.EstimatedDeliveryMinutes,
				Status = "Pending",
				IsOpen = false,
				AverageRating = 0,
				TotalReviews = 0
			};
			await _repo.AddAsync(restaurant);
			return restaurant.Id;
		}

		public async Task<bool> UpdateAsync(Guid id, CreateRestaurantDto dto)
		{
			var r = await _repo.GetByIdAsync(id);
			if (r == null)
			{
				return false;
			}

			r.Name = dto.Name;
			r.Description = dto.Description;
			r.Address = dto.Address;
			r.CuisineTypes = dto.CuisineTypes;
			r.LogoUrl = dto.LogoUrl;
			r.OperatingHours = dto.OperatingHours;
			r.MinimumOrderAmount = dto.MinimumOrderAmount;
			r.EstimatedDeliveryMinutes = dto.EstimatedDeliveryMinutes;

			await _repo.UpdateAsync(r);
			return true;
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var r = await _repo.GetByIdAsync(id);
			if (r == null)
			{
				return false;
			}
			await _repo.DeleteAsync(id);
			return true;
		}
	}
}
