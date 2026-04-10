using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTO;
using RestaurantService.Services;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestaurantService.Controllers
{
	//[Authorize]
	[Route("api/restaurants")]
	[ApiController]
	public class RestaurantController : ControllerBase
	{
		private readonly IRestaurantService _service;
		public RestaurantController(IRestaurantService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			return Ok(await _service.GetAllAsync());
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var data = await _service.GetByIdAsync(id);
			if(data == null)
			{
				return NotFound();
			}
			return Ok(data);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateRestaurantDto dto)
		{
			// Temporary owner ID until JWT is implemented
			Guid ownerId = Guid.NewGuid(); // or Guid.Empty

			var id = await _service.CreateAsync(dto, ownerId);
			return Ok(new { id });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, CreateRestaurantDto dto)
		{
			var result = await _service.UpdateAsync(id, dto);
			if (!result)
			{
				return NotFound();
			}
			return Ok("Updated");
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _service.DeleteAsync(id);
			if (!result)
			{
				return NotFound();
			}
			return Ok("Deleted");
		}
	}
}
