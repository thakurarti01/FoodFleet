namespace PaymentService.DTOs
{
	public class PaymentRequestDTO
	{
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }   // "Card" or "COD"

		// For card payments (Stripe)
		public string? CardToken { get; set; }

		// For email notification
		public string? CustomerEmail { get; set; }
		public string? CustomerName { get; set; }
		public List<string>? ItemNames { get; set; }
	}
}
