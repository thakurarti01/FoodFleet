using MenuService.Models;

namespace MenuService.Repositories
{
	public interface IMenuRepository
	{
		Task<IEnumerable<MenuCategory>> GetCategoriesAsync();
		Task<MenuCategory> GetCategoryByIdAsync(int id);
		Task<MenuCategory> AddCategoryAsync(MenuCategory category);
		Task UpdateCategoryAsync(MenuCategory category);
		Task DeleteCategoryAsync(int id);

		Task<IEnumerable<MenuItem>> GetItemsAsync(string? dietType = null, bool? available = null);
		Task<MenuItem> GetItemByIdAsync(int id);
		Task<MenuItem> AddItemAsync(MenuItem item);
		Task UpdateItemAsync(MenuItem item);
		Task DeleteItemAsync(int id);
		Task UpdateItemAvailabilityAsync(int id, bool isAvailable);

		Task<MenuItemOption> AddOptionAsync(MenuItemOption option);
		Task<IEnumerable<MenuItemOption>> GetOptionsByItemIdAsync(int itemId);
		Task DeleteOptionAsync(int optionId);

	}
}
