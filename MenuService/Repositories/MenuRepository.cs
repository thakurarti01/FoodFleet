using MenuService.Data;
using MenuService.Models;
using Microsoft.EntityFrameworkCore;

namespace MenuService.Repositories
{
	public class MenuRepository : IMenuRepository
	{
		private readonly MenuDbContext _context;
		public MenuRepository(MenuDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync()
		{
			return await _context.Categories.Include(c => c.Items).ToListAsync();
		}

		public async Task<MenuCategory> GetCategoryByIdAsync(int id)
		{
			return await _context.Categories.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);

		}

		public async Task<MenuCategory> AddCategoryAsync(MenuCategory category)
		{
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();
			return category;
		}

		public async Task UpdateCategoryAsync(MenuCategory category)
		{
			_context.Categories.Update(category);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteCategoryAsync(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category != null)
			{
				_context.Categories.Remove(category);
				await _context.SaveChangesAsync();
			}
		}

		public async Task<IEnumerable<MenuItem>> GetItemsAsync(string? dietType = null, bool? available = null)
		{
			var query = _context.Items.Include(i => i.Options).AsQueryable();
			if (dietType != null) query = query.Where(i => i.DietType == dietType);
			if (available.HasValue) query = query.Where(i => i.IsAvailable == available.Value);
			return await query.ToListAsync();
		}

		public async Task<MenuItem> GetItemByIdAsync(int id) =>
			await _context.Items.Include(i => i.Options).FirstOrDefaultAsync(i => i.Id == id);

		public async Task<MenuItem> AddItemAsync(MenuItem item)
		{
			_context.Items.Add(item);
			await _context.SaveChangesAsync();
			return item;
		}

		public async Task UpdateItemAsync(MenuItem item)
		{
			_context.Items.Update(item);
			await _context.SaveChangesAsync();
		}

		public async Task DeleteItemAsync(int id)
		{
			var item = await _context.Items.FindAsync(id);
			if (item != null)
			{
				_context.Items.Remove(item);
				await _context.SaveChangesAsync();
			}
		}

		public async Task UpdateItemAvailabilityAsync(int id, bool isAvailable)
		{
			var item = await _context.Items.FindAsync(id);
			if (item != null)
			{
				item.IsAvailable = isAvailable;
				await _context.SaveChangesAsync();
			}
		}

		public async Task<MenuItemOption> AddOptionAsync(MenuItemOption option)
		{
			_context.Options.Add(option);
			await _context.SaveChangesAsync();
			return option;
		}

		public async Task<IEnumerable<MenuItemOption>> GetOptionsByItemIdAsync(int itemId)
		{
			return await _context.Options
				.Where(o => o.MenuItemId == itemId)
				.ToListAsync();
		}

		public async Task DeleteOptionAsync(int optionId)
		{
			var option = await _context.Options.FindAsync(optionId);
			if (option != null)
			{
				_context.Options.Remove(option);
				await _context.SaveChangesAsync();
			}
		}

	}
}
