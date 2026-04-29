using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Exceptions;
using PaymentService.Interfaces;

namespace PaymentService.Controllers
{
    /// <summary>
    /// REST endpoints for payment operations.
    /// POST /api/payment/pay        — initiate payment for an order (COD or Card)
    /// GET  /api/payment/{id}       — get payment by payment ID
    /// GET  /api/payment/order/{id} — get payment by order ID
    /// POST /api/payment/refund     — admin-only refund
    /// </summary>
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("pay")]
        public async Task<IActionResult> Pay(PayRequestDto dto)
        {
            try
            {
                var result = await _service.PayAsync(dto);
                return Ok(result);
            }
            catch (PaymentAmountException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidPaymentMethodException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetById(int paymentId)
        {
            try
            {
                var result = await _service.GetByIdAsync(paymentId);
                return result == null
                    ? NotFound(new { message = $"Payment #{paymentId} not found." })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            try
            {
                var result = await _service.GetByOrderIdAsync(orderId);
                return result == null
                    ? NotFound(new { message = $"No payment found for order #{orderId}." })
                    : Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("refund")]
        public async Task<IActionResult> Refund(RefundDto dto)
        {
            try
            {
                await _service.RefundAsync(dto);
                return Ok("Refund processed.");
            }
            catch (PaymentNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (PaymentAlreadyRefundedException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
