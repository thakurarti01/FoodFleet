using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTO;
using RestaurantService.Services;
using System.Security.Claims;

namespace RestaurantService.Controllers
{
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
			// Extract owner ID from JWT claims
			var ownerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			Guid ownerId = ownerIdClaim != null ? Guid.Parse(ownerIdClaim) : Guid.Empty;

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
