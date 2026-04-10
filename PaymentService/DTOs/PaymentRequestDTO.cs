namespace PaymentService.DTOs
{
	public class PaymentRequestDTO
	{
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public string PaymentMethod { get; set; }   // "Card" or "COD"

		// For card payments (Stripe)
		public string CardToken { get; set; }       // Token from frontend (Stripe)
	}
}
