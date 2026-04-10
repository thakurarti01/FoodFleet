using Microsoft.AspNetCore.Mvc;
using SearchDiscoveryService.DTOs;
using SearchDiscoveryService.Interfaces;

namespace SearchDiscoveryService.Controllers
{
	[ApiController]
	[Route("api/search")]
	public class SearchController : ControllerBase
	{
		private readonly ISearchService _searchService;

		public SearchController(ISearchService searchService)
		{
			_searchService = searchService;
		}

		//  Search with filters
		[HttpPost]
		public async Task<IActionResult> Search([FromBody] SearchRequestDTO request)
		{
			if (request == null)
				return BadRequest("Invalid request");

			var result = await _searchService.SearchAsync(request);
			return Ok(result);
		}

		//  Promoted restaurants (homepage)
		[HttpGet("promoted")]
		public async Task<IActionResult> GetPromoted([FromQuery] double lat, [FromQuery] double lon)
		{
			var result = await _searchService.GetPromotedRestaurantsAsync(lat, lon);
			return Ok(result);
		}
	}
}