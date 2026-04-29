using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTOs;
using RestaurantService.Interfaces;
using System.Security.Claims;

namespace RestaurantService.Controllers
{
    /// <summary>
    /// REST endpoints for restaurant management.
    /// Public: browse approved restaurants. Owners: create/update/toggle open.
    /// Admin: approve/reject restaurants and see all regardless of status.
    /// </summary>
    [ApiController]
    [Route("api/restaurants")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _service;

        public RestaurantController(IRestaurantService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var all = await _service.GetAllAsync(adminView: true);
            var mine = all.Where(r => r.OwnerId == ownerId).ToList();
            return Ok(mine);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Admin sees all, public sees only Approved
            var isAdmin = User.IsInRole("Admin");
            return Ok(await _service.GetAllAsync(adminView: isAdmin));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var r = await _service.GetByIdAsync(id);
            return r == null ? NotFound() : Ok(r);
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateRestaurantDto dto)
        {
            var ownerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var id = await _service.CreateAsync(dto, ownerId);
            return Ok(new { id });
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CreateRestaurantDto dto)
        {
            var requesterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _service.UpdateAsync(id, dto, requesterId);
            return result ? Ok("Updated") : NotFound();
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok("Deleted") : NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _service.ApproveAsync(id);
            return result ? Ok("Approved") : NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, RejectRestaurantDto dto)
        {
            var result = await _service.RejectAsync(id, dto.Reason);
            return result ? Ok("Rejected") : NotFound();
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{id}/toggle-open")]
        public async Task<IActionResult> ToggleOpen(Guid id, [FromBody] ToggleOpenDto dto)
        {
            var result = await _service.ToggleOpenAsync(id, dto.IsOpen);
            return result ? Ok(dto.IsOpen ? "Restaurant is now Open" : "Restaurant is now Closed") : NotFound();
        }
    }
}

public class ToggleOpenDto
{
    public bool IsOpen { get; set; }
}
