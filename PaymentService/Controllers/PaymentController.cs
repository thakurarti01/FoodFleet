using Microsoft.AspNetCore.Mvc;
using PaymentService.Interfaces;
using PaymentService.DTOs;
using System.Threading.Tasks;

namespace PaymentService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PaymentController : ControllerBase
	{
		private readonly IPaymentService _paymentService;

		public PaymentController(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		// 1. Card Payment (Stripe)
		[HttpPost("card")]
		public async Task<IActionResult> PayByCard([FromBody] PaymentRequestDTO request)
		{
			var result = await _paymentService.ProcessCardPaymentAsync(request);
			return Ok(result);
		}

		// 2. Cash on Delivery
		[HttpPost("cod")]
		public async Task<IActionResult> PayByCOD([FromBody] PaymentRequestDTO request)
		{
			var result = await _paymentService.ProcessCODPaymentAsync(request);
			return Ok(result);
		}

		// 3. Get Payment by ID
		[HttpGet("{id}")]
		public async Task<IActionResult> GetPayment(int id)
		{
			var result = await _paymentService.GetPaymentByIdAsync(id);

			if (result == null)
				return NotFound("Payment not found");

			return Ok(result);
		}

		// 4. Refund
		[HttpPost("refund")]
		public async Task<IActionResult> Refund([FromBody] RefundRequestDTO request)
		{
			var success = await _paymentService.ProcessRefundAsync(request);

			if (!success)
				return BadRequest("Refund failed");

			return Ok("Refund processed successfully");
		}

		// 5. Download Invoice
		[HttpGet("invoice/{paymentId}")]
		public async Task<IActionResult> DownloadInvoice(int paymentId)
		{
			var pdfBytes = await _paymentService.GenerateInvoiceAsync(paymentId);

			if (pdfBytes == null || pdfBytes.Length == 0)
				return NotFound("Invoice not found");

			return File(pdfBytes, "application/pdf", $"Invoice_{paymentId}.pdf");
		}
	}
}