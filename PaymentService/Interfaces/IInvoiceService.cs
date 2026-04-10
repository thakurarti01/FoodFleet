using System.Threading.Tasks;

namespace PaymentService.Interfaces
{
	public interface IInvoiceService
	{
		Task<byte[]> GenerateInvoiceAsync(int paymentId);
	}
}