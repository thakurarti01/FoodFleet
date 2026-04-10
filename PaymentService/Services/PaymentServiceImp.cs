using PaymentService.Interfaces;
using PaymentService.DTOs;
using PaymentService.Models;
using PaymentService.Data;
using System;
using System.Threading.Tasks;

namespace PaymentService.Services
{
	public class PaymentServiceImp : IPaymentService
	{
		private readonly PaymentDbContext _context;
		private readonly IStripePaymentService _stripeService;
		private readonly IInvoiceService _invoiceService;

		public PaymentServiceImp(
			PaymentDbContext context,
			IStripePaymentService stripeService,
			IInvoiceService invoiceService)
		{
			_context = context;
			_stripeService = stripeService;
			_invoiceService = invoiceService;
		}

		public async Task<PaymentResponseDTO> ProcessCardPaymentAsync(PaymentRequestDTO request)
		{
			var transactionId = await _stripeService.CreatePaymentAsync(
				request.Amount, "INR", request.CardToken);

			var payment = new Payment
			{
				OrderId = request.OrderId,
				Amount = request.Amount,
				PaymentMethod = "Card",
				Status = "Completed",
				TransactionId = transactionId,
				CreatedAt = DateTime.UtcNow
			};

			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();

			return new PaymentResponseDTO
			{
				PaymentId = payment.Id,
				OrderId = payment.OrderId,
				Amount = payment.Amount,
				Status = payment.Status,
				TransactionId = payment.TransactionId,
				Message = "Card payment successful"
			};
		}

		public async Task<PaymentResponseDTO> ProcessCODPaymentAsync(PaymentRequestDTO request)
		{
			var payment = new Payment
			{
				OrderId = request.OrderId,
				Amount = request.Amount,
				PaymentMethod = "COD",
				Status = "Pending",
				CreatedAt = DateTime.UtcNow
			};

			_context.Payments.Add(payment);
			await _context.SaveChangesAsync();

			return new PaymentResponseDTO
			{
				PaymentId = payment.Id,
				OrderId = payment.OrderId,
				Amount = payment.Amount,
				Status = payment.Status,
				Message = "COD order placed"
			};
		}

		public async Task<PaymentResponseDTO> GetPaymentByIdAsync(int paymentId)
		{
			var payment = await _context.Payments.FindAsync(paymentId);

			if (payment == null)
				return null;

			return new PaymentResponseDTO
			{
				PaymentId = payment.Id,
				OrderId = payment.OrderId,
				Amount = payment.Amount,
				Status = payment.Status,
				TransactionId = payment.TransactionId
			};
		}

		public async Task<bool> ProcessRefundAsync(RefundRequestDTO request)
		{
			var payment = await _context.Payments.FindAsync(request.PaymentId);

			if (payment == null)
				return false;

			if (payment.PaymentMethod == "Card")
			{
				await _stripeService.RefundPaymentAsync(payment.TransactionId);
			}

			var refund = new Refund
			{
				PaymentId = request.PaymentId,
				Amount = request.Amount,
				Status = "Processed",
				Reason = request.Reason,
				CreatedAt = DateTime.UtcNow,
				ProcessedAt = DateTime.UtcNow
			};

			_context.Refunds.Add(refund);

			payment.Status = "Refunded";
			payment.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			return true;
		}

		public async Task<byte[]> GenerateInvoiceAsync(int paymentId)
		{
			return await _invoiceService.GenerateInvoiceAsync(paymentId);
		}
	}
}