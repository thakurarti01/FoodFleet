using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SearchDiscoveryService.Data;
using SearchDiscoveryService.DTOs;
using SearchDiscoveryService.Interfaces;
using SearchDiscoveryService.Models;
using SearchDiscoveryService.Utilities;

namespace SearchDiscoveryService.Services
{
	public class SearchService : ISearchService
	{
		private readonly SearchDbContext _context;
		private readonly IMapper _mapper;

		public SearchService(SearchDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		//  MAIN SEARCH
		public async Task<List<SearchResultDTO>> SearchAsync(SearchRequestDTO request)
		{
			var query = _context.Restaurants
				.Include(r => r.MenuItems)
				.AsQueryable();

			//  Full-text search (name + menu items)
			if (!string.IsNullOrEmpty(request.Query))
			{
				query = query.Where(r =>
					r.Name.Contains(request.Query) ||
					r.MenuItems.Any(m => m.Name.Contains(request.Query)));
			}

			//  Filters
			if (!string.IsNullOrEmpty(request.CuisineType))
				query = query.Where(r => r.CuisineType == request.CuisineType);

			if (request.MinRating.HasValue)
				query = query.Where(r => r.Rating >= request.MinRating.Value);

			if (request.MaxPrice.HasValue)
				query = query.Where(r => r.PriceAverage <= request.MaxPrice.Value);

			if (request.MaxDeliveryTime.HasValue)
				query = query.Where(r => r.DeliveryTimeMinutes <= request.MaxDeliveryTime.Value);

			var restaurants = await query.ToListAsync();

			//  Map to DTO
			var results = _mapper.Map<List<SearchResultDTO>>(restaurants);

			//  Add distance
			foreach (var r in results)
			{
				var entity = restaurants.First(x => x.Id == r.Id);

				r.DistanceKm = GeoUtils.GetDistance(
					request.Latitude,
					request.Longitude,
					entity.Latitude,
					entity.Longitude
				);
			}

			//  Sort: promoted first → then nearest
			return results
				.OrderByDescending(r => r.IsPromoted)
				.ThenBy(r => r.DistanceKm)
				.ToList();
		}

		//  Promoted Restaurants (Homepage)
		public async Task<List<SearchResultDTO>> GetPromotedRestaurantsAsync(double lat, double lon)
		{
			var restaurants = await _context.Restaurants
				.Where(r => r.IsPromoted)
				.ToListAsync();

			var results = _mapper.Map<List<SearchResultDTO>>(restaurants);

			foreach (var r in results)
			{
				var entity = restaurants.First(x => x.Id == r.Id);

				r.DistanceKm = GeoUtils.GetDistance(lat, lon, entity.Latitude, entity.Longitude);
			}

			return results.OrderBy(r => r.DistanceKm).ToList();
		}

		// Cuisine Categories
		public async Task<List<BrowseCategoryDTO>> GetCuisineCategoriesAsync()
		{
			return await _context.Restaurants
				.GroupBy(r => r.CuisineType)
				.Select(g => new BrowseCategoryDTO
				{
					CuisineType = g.Key,
					RestaurantCount = g.Count()
				})
				.ToListAsync();
		}
	}
}