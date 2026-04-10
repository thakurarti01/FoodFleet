using Microsoft.AspNetCore.Mvc;
using SearchDiscoveryService.DTOs;
using SearchDiscoveryService.Interfaces;

namespace SearchDiscoveryService.Controllers
{
	[ApiController]
	[Route("api/browse")]
	public class BrowseController : ControllerBase
	{
		private readonly ISearchService _searchService;

		public BrowseController(ISearchService searchService)
		{
			_searchService = searchService;
		}

		//  Get all cuisine categories
		[HttpGet("categories")]
		public async Task<IActionResult> GetCategories()
		{
			var result = await _searchService.GetCuisineCategoriesAsync();
			return Ok(result);
		}

		//  Get restaurants by cuisine
		[HttpGet("restaurants")]
		public async Task<IActionResult> GetRestaurantsByCuisine(
			[FromQuery] string cuisine,
			[FromQuery] double lat,
			[FromQuery] double lon)
		{
			if (string.IsNullOrEmpty(cuisine))
				return BadRequest("Cuisine is required");

			var request = new SearchRequestDTO
			{
				CuisineType = cuisine,
				Latitude = lat,
				Longitude = lon
			};

			var result = await _searchService.SearchAsync(request);
			return Ok(result);
		}
	}
}