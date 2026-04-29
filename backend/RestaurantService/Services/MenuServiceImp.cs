using Microsoft.EntityFrameworkCore;
using RestaurantService.Data;
using RestaurantService.DTOs;
using RestaurantService.Interfaces;
using RestaurantService.Models;

namespace RestaurantService.Services
{
    /// <summary>
    /// Manages menu items and categories for restaurants.
    /// Owners can add/update/delete items; only available items are returned to customers.
    /// </summary>
    public class MenuServiceImp : IMenuService
    {
        private readonly RestaurantDbContext _context;

        public MenuServiceImp(RestaurantDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MenuItem>> GetByRestaurantAsync(Guid restaurantId)
        {
            return await _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.RestaurantId == restaurantId && m.IsAvailable)
                .ToListAsync();
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            return await _context.MenuItems.Include(m => m.Category).FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MenuItem> CreateAsync(Guid restaurantId, CreateMenuItemDto dto)
        {
            var item = new MenuItem
            {
                RestaurantId = restaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                DietType = dto.DietType,
                CategoryId = dto.CategoryId,
                IsAvailable = true
            };
            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateAsync(int id, UpdateMenuItemDto dto)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return false;

            if (dto.Name != null) item.Name = dto.Name;
            if (dto.Description != null) item.Description = dto.Description;
            if (dto.Price.HasValue) item.Price = dto.Price.Value;
            if (dto.ImageUrl != null) item.ImageUrl = dto.ImageUrl;
            if (dto.IsAvailable.HasValue) item.IsAvailable = dto.IsAvailable.Value;
            if (dto.DietType != null) item.DietType = dto.DietType;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.MenuItems.FindAsync(id);
            if (item == null) return false;
            _context.MenuItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MenuCategory>> GetCategoriesAsync()
        {
            return await _context.MenuCategories.ToListAsync();
        }

        public async Task<MenuCategory> CreateCategoryAsync(string name, string description)
        {
            var cat = new MenuCategory { Name = name, Description = description };
            _context.MenuCategories.Add(cat);
            await _context.SaveChangesAsync();
            return cat;
        }
    }
}
