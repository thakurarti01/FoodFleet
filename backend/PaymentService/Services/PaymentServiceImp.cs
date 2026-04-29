using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.DTOs;
using PaymentService.Exceptions;
using PaymentService.Interfaces;
using PaymentService.Models;

namespace PaymentService.Services
{
    /// <summary>
    /// Handles payment processing for orders.
    /// Supports COD (Cash on Delivery) and Card (simulated) payment methods.
    /// Card payments are immediately marked Success; COD stays Pending until delivery.
    /// Also handles refunds for admin-initiated cancellations.
    /// </summary>
    public class PaymentServiceImp : IPaymentService
    {
        private readonly PaymentDbContext _context;

        public PaymentServiceImp(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponseDto> PayAsync(PayRequestDto dto)
        {
            try
            {
                if (dto.Amount <= 0)
                    throw new PaymentAmountException();

                var validMethods = new[] { "COD", "Card" };
                if (!validMethods.Contains(dto.Method))
                    throw new InvalidPaymentMethodException(dto.Method);

                var status = dto.Method == "Card" ? "Success" : "Pending";
                var transactionId = dto.Method == "Card"
                    ? $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}"
                    : null;

                var payment = new Payment
                {
                    OrderId = dto.OrderId,
                    UserId = dto.UserId,
                    Amount = dto.Amount,
                    Method = dto.Method,
                    Status = status,
                    TransactionId = transactionId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                return new PaymentResponseDto
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    Method = payment.Method,
                    TransactionId = payment.TransactionId,
                    Message = dto.Method == "Card" ? "Card payment simulated successfully." : "COD order placed."
                };
            }
            catch (PaymentAmountException) { throw; }
            catch (InvalidPaymentMethodException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Payment processing failed: {ex.Message}", ex);
            }
        }

        public async Task<PaymentResponseDto?> GetByIdAsync(int paymentId)
        {
            try
            {
                var p = await _context.Payments.FindAsync(paymentId);
                return p == null ? null : MapToDto(p);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve payment #{paymentId}: {ex.Message}", ex);
            }
        }

        public async Task<PaymentResponseDto?> GetByOrderIdAsync(int orderId)
        {
            try
            {
                var p = await _context.Payments.FirstOrDefaultAsync(x => x.OrderId == orderId);
                return p == null ? null : MapToDto(p);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve payment for order #{orderId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> RefundAsync(RefundDto dto)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(dto.PaymentId)
                    ?? throw new PaymentNotFoundException(dto.PaymentId);

                if (payment.Status == "Refunded")
                    throw new PaymentAlreadyRefundedException(dto.PaymentId);

                payment.Status = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (PaymentNotFoundException) { throw; }
            catch (PaymentAlreadyRefundedException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Refund failed: {ex.Message}", ex);
            }
        }

        private static PaymentResponseDto MapToDto(Payment p) => new()
        {
            PaymentId = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Status = p.Status,
            Method = p.Method,
            TransactionId = p.TransactionId,
            Message = string.Empty
        };
    }
}
