using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/delivery-agents/{agentId}/ratings")]
    public class DeliveryAgentRatingController : ControllerBase
    {
        private readonly IDeliveryAgentRatingService _service;

        public DeliveryAgentRatingController(IDeliveryAgentRatingService service)
        {
            _service = service;
        }

        /// <summary>
        /// Customer rates a delivery agent after order delivery
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(Guid agentId, [FromBody] CreateDeliveryAgentRatingDto dto)
        {
            try
            {
                var rating = await _service.CreateAsync(agentId, dto);
                return Ok(new DeliveryAgentRatingResponseDto
                {
                    Id = rating.Id,
                    DeliveryAgentId = rating.DeliveryAgentId,
                    CustomerId = rating.CustomerId,
                    OrderId = rating.OrderId,
                    Rating = rating.Rating,
                    Comment = rating.Comment,
                    CreatedAt = rating.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all ratings for a delivery agent
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetByAgent(Guid agentId)
        {
            try
            {
                var ratings = await _service.GetByAgentAsync(agentId);
                var response = ratings.Select(r => new DeliveryAgentRatingResponseDto
                {
                    Id = r.Id,
                    DeliveryAgentId = r.DeliveryAgentId,
                    CustomerId = r.CustomerId,
                    OrderId = r.OrderId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                });
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get average rating for a delivery agent
        /// </summary>
        [HttpGet("average")]
        public async Task<IActionResult> GetAverageRating(Guid agentId)
        {
            try
            {
                var average = await _service.GetAverageRatingAsync(agentId);
                return Ok(new { agentId, averageRating = Math.Round(average, 2) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
