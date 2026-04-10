using System.Threading.Tasks;

namespace PaymentService.Interfaces
{
	public interface IStripePaymentService
	{
		Task<string> CreatePaymentAsync(decimal amount, string currency, string sourceToken);
		Task<bool> RefundPaymentAsync(string transactionId);
	}
}