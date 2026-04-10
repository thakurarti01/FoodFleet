using PaymentService.Interfaces;
using PaymentService.Utilities;
using System.Threading.Tasks;

namespace PaymentService.Services
{
	public class InvoiceService : IInvoiceService
	{
		public async Task<byte[]> GenerateInvoiceAsync(int paymentId)
		{
			var content = $"Invoice for Payment ID: {paymentId}";
			return await Task.FromResult(PdfGenerator.GenerateInvoice(content));
		}
	}
}