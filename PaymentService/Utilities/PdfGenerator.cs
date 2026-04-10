using System.Text;

namespace PaymentService.Utilities
{
	public static class PdfGenerator
	{
		public static byte[] GenerateInvoice(string content)
		{
			// Temporary (text as PDF simulation)
			return Encoding.UTF8.GetBytes(content);
		}
	}
}