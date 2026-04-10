namespace PaymentService.Models
{
	public class Payment
	{
		public int Id { get; set; }                     // Primary key
		public int OrderId { get; set; }                // Associated order
		public decimal Amount { get; set; }             // Payment amount
		public string PaymentMethod { get; set; }       // "Card" or "COD"
		public string Status { get; set; }              // "Pending", "Completed", "Failed"
		public DateTime CreatedAt { get; set; }         // Timestamp of payment
		public DateTime? UpdatedAt { get; set; }        // Timestamp of last update
		public string? TransactionId { get; set; }       // For card payments (Stripe)

	}
}
