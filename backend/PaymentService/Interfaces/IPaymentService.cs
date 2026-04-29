using PaymentService.DTOs;

namespace PaymentService.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> PayAsync(PayRequestDto dto);
        Task<PaymentResponseDto?> GetByIdAsync(int paymentId);
        Task<PaymentResponseDto?> GetByOrderIdAsync(int orderId);
        Task<bool> RefundAsync(RefundDto dto);
    }
}
