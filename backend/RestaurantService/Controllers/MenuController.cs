using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTOs;
using RestaurantService.Interfaces;

namespace RestaurantService.Controllers
{
    [ApiController]
    [Route("api/restaurants")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("{restaurantId}/menu")]
        public async Task<IActionResult> GetMenu(Guid restaurantId)
            => Ok(await _menuService.GetByRestaurantAsync(restaurantId));

        [HttpGet("menu/{itemId}")]
        public async Task<IActionResult> GetItem(int itemId)
        {
            var item = await _menuService.GetByIdAsync(itemId);
            return item == null ? NotFound() : Ok(item);
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("{restaurantId}/menu")]
        public async Task<IActionResult> CreateItem(Guid restaurantId, CreateMenuItemDto dto)
        {
            var item = await _menuService.CreateAsync(restaurantId, dto);
            return Ok(item);
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("menu/{itemId}")]
        public async Task<IActionResult> UpdateItem(int itemId, UpdateMenuItemDto dto)
        {
            var result = await _menuService.UpdateAsync(itemId, dto);
            return result ? Ok("Updated") : NotFound();
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpDelete("menu/{itemId}")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var result = await _menuService.DeleteAsync(itemId);
            return result ? Ok("Deleted") : NotFound();
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
            => Ok(await _menuService.GetCategoriesAsync());

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var cat = await _menuService.CreateCategoryAsync(dto.Name, dto.Description);
            return Ok(cat);
        }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
