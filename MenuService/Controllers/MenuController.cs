using MenuService.Models;
using MenuService.Services;
using Microsoft.AspNetCore.Mvc;
using MenuService.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MenuService.Controllers
{
	[Route("api/menu")]
	[ApiController]
	public class MenuController : ControllerBase
	{
		private readonly IMenuService _service;

		public MenuController(IMenuService service)
		{
			_service = service;
		}

		// CATEGORY APIs

		[HttpGet("categories")]
		public async Task<IActionResult> GetCategories()
		{
			var data = await _service.GetCategoriesAsync();
			return Ok(data);
		}

		[HttpPost("categories")]
		public async Task<IActionResult> CreateCategory(MenuCategory category)
		{
			var result = await _service.CreateCategoryAsync(category);
			return Ok(result);
		}

		[HttpPut("categories/{id}")]
		public async Task<IActionResult> UpdateCategory(int id, MenuCategory category)
		{
			await _service.UpdateCategoryAsync(id, category);
			return Ok();
		}

		[HttpDelete("categories/{id}")]
		public async Task<IActionResult> DeleteCategory(int id)
		{
			await _service.DeleteCategoryAsync(id);
			return Ok();
		}

		// ITEM APIs

		[HttpGet("items")]
		public async Task<IActionResult> GetItems(string? dietType, bool? available)
		{
			var data = await _service.GetItemsAsync(dietType, available);
			return Ok(data);
		}

		[HttpGet("items/{id}")]
		public async Task<IActionResult> GetItem(int id)
		{
			var item = await _service.GetItemByIdAsync(id);
			return Ok(item);
		}

		[HttpPost("items")]
		public async Task<IActionResult> CreateItem(MenuItem item)
		{
			var result = await _service.CreateItemAsync(item);
			return Ok(result);
		}

		[HttpPut("items/{id}")]
		public async Task<IActionResult> UpdateItem(int id, MenuItem item)
		{
			await _service.UpdateItemAsync(id, item);
			return Ok();
		}

		[HttpPatch("items/{id}/availability")]
		public async Task<IActionResult> UpdateAvailability(int id, bool isAvailable)
		{
			await _service.UpdateAvailabilityAsync(id, isAvailable);
			return Ok();
		}

		[HttpDelete("items/{id}")]
		public async Task<IActionResult> DeleteItem(int id)
		{
			await _service.DeleteItemAsync(id);
			return Ok();
		}


		// Add option
		[HttpPost("items/{itemId}/options")]
		public async Task<IActionResult> AddOption(int itemId, MenuItemOption option)
		{
			var result = await _service.AddOptionAsync(itemId, option);
			return Ok(result);
		}

		// Get options
		[HttpGet("items/{itemId}/options")]
		public async Task<IActionResult> GetOptions(int itemId)
		{
			var data = await _service.GetOptionsAsync(itemId);
			return Ok(data);
		}

		// Delete option
		[HttpDelete("options/{optionId}")]
		public async Task<IActionResult> DeleteOption(int optionId)
		{
			await _service.DeleteOptionAsync(optionId);
			return Ok();
		}

		//-------------for discount ----------------------
		[HttpPatch("items/{id}/discount")]
		public async Task<IActionResult> SetDiscount(int id, DiscountDTO dto)
		{
			await _service.SetDiscountAsync(id, dto);
			return Ok();
		}
	}
}
