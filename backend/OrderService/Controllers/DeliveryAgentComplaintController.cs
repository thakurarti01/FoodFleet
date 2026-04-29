using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/delivery-agents/{agentId}/complaints")]
    public class DeliveryAgentComplaintController : ControllerBase
    {
        private readonly IDeliveryAgentComplaintService _service;

        public DeliveryAgentComplaintController(IDeliveryAgentComplaintService service)
        {
            _service = service;
        }

        /// <summary>
        /// Customer files a complaint against a delivery agent
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(Guid agentId, [FromBody] CreateDeliveryAgentComplaintDto dto)
        {
            try
            {
                var complaint = await _service.CreateAsync(agentId, dto);
                return Ok(new DeliveryAgentComplaintResponseDto
                {
                    Id = complaint.Id,
                    DeliveryAgentId = complaint.DeliveryAgentId,
                    CustomerId = complaint.CustomerId,
                    OrderId = complaint.OrderId,
                    ComplaintType = complaint.ComplaintType,
                    Description = complaint.Description,
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
        /// Get all complaints for a delivery agent (Agent/Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "DeliveryAgent,Admin")]
        public async Task<IActionResult> GetByAgent(Guid agentId)
        {
            try
            {
                var complaints = await _service.GetByAgentAsync(agentId);
                var response = complaints.Select(c => new DeliveryAgentComplaintResponseDto
                {
                    Id = c.Id,
                    DeliveryAgentId = c.DeliveryAgentId,
                    CustomerId = c.CustomerId,
                    OrderId = c.OrderId,
                    ComplaintType = c.ComplaintType,
                    Description = c.Description,
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
    [Route("api/agent-complaints")]
    public class AdminAgentComplaintController : ControllerBase
    {
        private readonly IDeliveryAgentComplaintService _service;

        public AdminAgentComplaintController(IDeliveryAgentComplaintService service)
        {
            _service = service;
        }

        /// <summary>
        /// Admin gets all complaints across all delivery agents
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var complaints = await _service.GetAllAsync();
                var response = complaints.Select(c => new DeliveryAgentComplaintResponseDto
                {
                    Id = c.Id,
                    DeliveryAgentId = c.DeliveryAgentId,
                    CustomerId = c.CustomerId,
                    OrderId = c.OrderId,
                    ComplaintType = c.ComplaintType,
                    Description = c.Description,
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
        public async Task<IActionResult> Resolve(int complaintId, [FromBody] ResolveDeliveryAgentComplaintDto dto)
        {
            try
            {
                var complaint = await _service.ResolveAsync(complaintId, dto);
                return Ok(new DeliveryAgentComplaintResponseDto
                {
                    Id = complaint.Id,
                    DeliveryAgentId = complaint.DeliveryAgentId,
                    CustomerId = complaint.CustomerId,
                    OrderId = complaint.OrderId,
                    ComplaintType = complaint.ComplaintType,
                    Description = complaint.Description,
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
