using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.DTOs;
using RestaurantService.Interfaces;

namespace RestaurantService.Controllers
{
    [ApiController]
    [Route("api/restaurants/{restaurantId}/complaints")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;

        public ComplaintController(IComplaintService service)
        {
            _service = service;
        }

        /// <summary>
        /// Customer files a complaint against a restaurant
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(Guid restaurantId, [FromBody] CreateComplaintDto dto)
        {
            try
            {
                var complaint = await _service.CreateAsync(restaurantId, dto);
                return Ok(new ComplaintResponseDto
                {
                    Id = complaint.Id,
                    RestaurantId = complaint.RestaurantId,
                    CustomerId = complaint.CustomerId,
                    OrderId = complaint.OrderId,
                    ComplaintType = complaint.ComplaintType,
                    Description = complaint.Description,
                    ImageUrl = complaint.ImageUrl,
                    Status = complaint.Status,
                    AdminResponse = complaint.AdminResponse,
                    ResolvedAt = complaint.ResolvedAt,
                    CreatedAt = complaint.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all complaints for a restaurant (Owner/Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            try
            {
                var complaints = await _service.GetByRestaurantAsync(restaurantId);
                var response = complaints.Select(c => new ComplaintResponseDto
                {
                    Id = c.Id,
                    RestaurantId = c.RestaurantId,
                    CustomerId = c.CustomerId,
                    OrderId = c.OrderId,
                    ComplaintType = c.ComplaintType,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Status = c.Status,
                    AdminResponse = c.AdminResponse,
                    ResolvedAt = c.ResolvedAt,
                    CreatedAt = c.CreatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/complaints")]
    public class AdminComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;

        public AdminComplaintController(IComplaintService service)
        {
            _service = service;
        }

        /// <summary>
        /// Admin gets all complaints across all restaurants
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var complaints = await _service.GetAllAsync();
                var response = complaints.Select(c => new ComplaintResponseDto
                {
                    Id = c.Id,
                    RestaurantId = c.RestaurantId,
                    CustomerId = c.CustomerId,
                    OrderId = c.OrderId,
                    ComplaintType = c.ComplaintType,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Status = c.Status,
                    AdminResponse = c.AdminResponse,
                    ResolvedAt = c.ResolvedAt,
                    CreatedAt = c.CreatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Admin resolves or dismisses a complaint
        /// </summary>
        [HttpPut("{complaintId}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Resolve(int complaintId, [FromBody] ResolveComplaintDto dto)
        {
            try
            {
                var complaint = await _service.ResolveAsync(complaintId, dto);
                return Ok(new ComplaintResponseDto
                {
                    Id = complaint.Id,
                    RestaurantId = complaint.RestaurantId,
                    CustomerId = complaint.CustomerId,
                    OrderId = complaint.OrderId,
                    ComplaintType = complaint.ComplaintType,
                    Description = complaint.Description,
                    ImageUrl = complaint.ImageUrl,
                    Status = complaint.Status,
                    AdminResponse = complaint.AdminResponse,
                    ResolvedAt = complaint.ResolvedAt,
                    CreatedAt = complaint.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
