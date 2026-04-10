namespace PaymentService.Models
{
	public class Refund
	{
		public int Id { get; set; }                     // Primary key
		public int PaymentId { get; set; }              // Associated payment
		public decimal Amount { get; set; }             // Refund amount
		public string Status { get; set; }              // "Pending", "Processed", "Failed"
		public DateTime CreatedAt { get; set; }         // Timestamp of refund request
		public DateTime? ProcessedAt { get; set; }      // Timestamp when refund processed
		public string Reason { get; set; }              // Reason for refund (e.g., "Order Cancelled")
	}
}
