using PaymentService.DTOs;
using System.Threading.Tasks;

namespace PaymentService.Interfaces
{
	public interface IPaymentService
	{
		Task<PaymentResponseDTO> ProcessCardPaymentAsync(PaymentRequestDTO request);
		Task<PaymentResponseDTO> ProcessCODPaymentAsync(PaymentRequestDTO request);
		Task<PaymentResponseDTO> GetPaymentByIdAsync(int paymentId);
		Task<bool> ProcessRefundAsync(RefundRequestDTO request);
		Task<byte[]> GenerateInvoiceAsync(int paymentId);
	}
}