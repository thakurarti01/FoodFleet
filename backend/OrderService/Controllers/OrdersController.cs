using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Exceptions;
using OrderService.Interfaces;

namespace OrderService.Controllers
{
    /// <summary>
    /// REST endpoints for order lifecycle management.
    /// Customers can place and cancel orders; owners/admins can update status;
    /// delivery agents can update delivery status (triggers delivered notification).
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _service;

        public OrdersController(IOrderService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(PlaceOrderDto dto)
        {
            try
            {
                var order = await _service.PlaceOrderAsync(dto);
                return CreatedAtAction(nameof(GetById), new { orderId = order.Id }, order);
            }
            catch (EmptyOrderException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(int orderId)
        {
            try
            {
                var order = await _service.GetByIdAsync(orderId);
                return order == null ? NotFound(new { message = $"Order #{orderId} not found." }) : Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            try
            {
                return Ok(await _service.GetByUserAsync(userId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("restaurant/{restaurantId}")]
        public async Task<IActionResult> GetByRestaurant(Guid restaurantId)
        {
            try
            {
                return Ok(await _service.GetByRestaurantAsync(restaurantId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "DeliveryAgent,Admin")]
        [HttpGet("agent/{agentId}")]
        public async Task<IActionResult> GetByAgent(Guid agentId)
        {
            try
            {
                return Ok(await _service.GetByAgentAsync(agentId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Returns orders with no delivery agent assigned yet. Admin use.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("unassigned")]
        public async Task<IActionResult> GetUnassigned()
        {
            try
            {
                return Ok(await _service.GetUnassignedAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>Returns orders currently out for delivery (Assigned or PickedUp). Admin use.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("active-deliveries")]
        public async Task<IActionResult> GetActiveDeliveries()
        {
            try
            {
                return Ok(await _service.GetActiveDeliveriesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{orderId}/cancel")]
        public async Task<IActionResult> Cancel(int orderId, [FromBody] CancelOrderDto dto)
        {
            try
            {
                await _service.CancelAsync(orderId, dto.Reason);
                return Ok("Cancelled");
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (OrderAlreadyCancelledException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (OrderNotCancellableException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateStatus(int orderId, UpdateOrderStatusDto dto)
        {
            try
            {
                await _service.UpdateStatusAsync(orderId, dto.Status);
                return Ok("Status updated");
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (OrderAlreadyCancelledException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{orderId}/assign-agent")]
        public async Task<IActionResult> AssignAgent(int orderId, AssignAgentDto dto)
        {
            try
            {
                await _service.AssignAgentAsync(orderId, dto.DeliveryAgentId);
                return Ok("Agent assigned");
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "DeliveryAgent,Admin")]
        [HttpPut("{orderId}/delivery-status")]
        public async Task<IActionResult> UpdateDeliveryStatus(int orderId, UpdateDeliveryStatusDto dto)
        {
            try
            {
                await _service.UpdateDeliveryStatusAsync(orderId, dto.DeliveryStatus);
                return Ok("Delivery status updated");
            }
            catch (OrderNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("restaurant/{restaurantId}/earnings")]
        public async Task<IActionResult> GetRestaurantEarnings(Guid restaurantId)
        {
            try
            {
                var earnings = await _service.GetRestaurantEarningsAsync(restaurantId);
                return Ok(new { restaurantId, totalEarnings = earnings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "DeliveryAgent,Admin")]
        [HttpGet("agent/{agentId}/earnings")]
        public async Task<IActionResult> GetAgentEarnings(Guid agentId)
        {
            try
            {
                var earnings = await _service.GetAgentEarningsAsync(agentId);
                return Ok(new { agentId, totalEarnings = earnings });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "DeliveryAgent")]
        [HttpPost("{orderId}/verify-otp")]
        public async Task<IActionResult> VerifyOTP(int orderId, [FromBody] VerifyOTPDto dto)
        {
            try
            {
                var order = await _service.GetByIdAsync(orderId);
                if (order == null) return NotFound(new { message = "Order not found" });
                
                if (string.IsNullOrEmpty(order.DeliveryOTP))
                {
                    return BadRequest(new { message = "No OTP generated for this order" });
                }
                
                if (order.DeliveryOTP != dto.OTP)
                {
                    return BadRequest(new { message = "Invalid OTP" });
                }
                
                // Check if OTP expired (30 minutes)
                if (order.OTPGeneratedAt.HasValue && 
                    DateTime.UtcNow - order.OTPGeneratedAt.Value > TimeSpan.FromMinutes(30))
                {
                    return BadRequest(new { message = "OTP expired. Please request a new one." });
                }
                
                return Ok(new { message = "OTP verified successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
