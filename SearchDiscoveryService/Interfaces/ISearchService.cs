using SearchDiscoveryService.DTOs;

namespace SearchDiscoveryService.Interfaces
{
	public interface ISearchService
	{
		Task<List<SearchResultDTO>> SearchAsync(SearchRequestDTO request);

		Task<List<SearchResultDTO>> GetPromotedRestaurantsAsync(double lat, double lon);

		Task<List<BrowseCategoryDTO>> GetCuisineCategoriesAsync();
	}
}