using MenuService.DTOs;
using MenuService.Models;

namespace MenuService.Services
{
	public interface IMenuService
	{
		Task<IEnumerable<MenuCategory>> GetCategoriesAsync();
		Task<MenuCategory> CreateCategoryAsync(MenuCategory category);
		Task UpdateCategoryAsync(int id, MenuCategory category);
		Task DeleteCategoryAsync(int id);

		Task<IEnumerable<MenuItem>> GetItemsAsync(string? dietType, bool? available);
		Task<MenuItem> GetItemByIdAsync(int id);
		Task<MenuItem> CreateItemAsync(MenuItem item);
		Task UpdateItemAsync(int id, MenuItem item);
		Task DeleteItemAsync(int id);
		Task UpdateAvailabilityAsync(int id, bool isAvailable);

		Task<MenuItemOption> AddOptionAsync(int itemId, MenuItemOption option);
		Task<IEnumerable<MenuItemOption>> GetOptionsAsync(int itemId);
		Task DeleteOptionAsync(int optionId);

		//----------discount-------------
		Task SetDiscountAsync(int id, DiscountDTO dto);
	}
}
