using MenuService.DTOs;
using MenuService.Models;
using MenuService.Repositories;

namespace MenuService.Services
{
	public class MenuServiceImp : IMenuService
	{
		private readonly IMenuRepository _repo;

		public MenuServiceImp(IMenuRepository repo)
		{
			_repo = repo;
		}

		public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync()
		{
			return await _repo.GetCategoriesAsync();
		}

		public async Task<MenuCategory> CreateCategoryAsync(MenuCategory category)
		{
			return await _repo.AddCategoryAsync(category);
		}

		public async Task UpdateCategoryAsync(int id, MenuCategory category)
		{
			category.Id = id;
			await _repo.UpdateCategoryAsync(category);
		}

		public async Task DeleteCategoryAsync(int id)
		{
			await _repo.DeleteCategoryAsync(id);
		}

		public async Task<IEnumerable<MenuItem>> GetItemsAsync(string? dietType, bool? available)
		{
			var items = await _repo.GetItemsAsync(dietType, available);

			var now = DateTime.Now;

			foreach (var item in items)
			{
				if (item.DiscountPercentage.HasValue &&
					item.DiscountStart <= now &&
					item.DiscountEnd >= now)
				{
					item.Price = item.Price -
						(item.Price * item.DiscountPercentage.Value / 100);
				}
			}

			return items;
		}

		public async Task<MenuItem> GetItemByIdAsync(int id)
		{
			return await _repo.GetItemByIdAsync(id);
		}

		public async Task<MenuItem> CreateItemAsync(MenuItem item)
		{
			return await _repo.AddItemAsync(item);
		}

		public async Task UpdateItemAsync(int id, MenuItem item)
		{
			item.Id = id;
			await _repo.UpdateItemAsync(item);
		}

		public async Task DeleteItemAsync(int id)
		{
			await _repo.DeleteItemAsync(id);
		}

		public async Task UpdateAvailabilityAsync(int id, bool isAvailable)
		{
			await _repo.UpdateItemAvailabilityAsync(id, isAvailable);
		}


		public async Task<MenuItemOption> AddOptionAsync(int itemId, MenuItemOption option)
		{
			var item = await _repo.GetItemByIdAsync(itemId);
			if (item == null)
				throw new Exception("Item not found");

			option.MenuItemId = itemId;
			return await _repo.AddOptionAsync(option);
		}

		public async Task<IEnumerable<MenuItemOption>> GetOptionsAsync(int itemId)
		{
			return await _repo.GetOptionsByItemIdAsync(itemId);
		}

		public async Task DeleteOptionAsync(int optionId)
		{
			await _repo.DeleteOptionAsync(optionId);
		}


		//-------------dicount service------------------
		public async Task SetDiscountAsync(int id, DiscountDTO dto)
		{
			var item = await _repo.GetItemByIdAsync(id);

			item.DiscountPercentage = dto.DiscountPercentage;
			item.DiscountStart = dto.Start;
			item.DiscountEnd = dto.End;

			await _repo.UpdateItemAsync(item);
		}
	}
}
