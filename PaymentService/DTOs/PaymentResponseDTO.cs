namespace PaymentService.DTOs
{
	public class PaymentResponseDTO
	{
		public int PaymentId { get; set; }
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public string Status { get; set; }          // "Success", "Failed"
		public string TransactionId { get; set; }  // For card payments
		public string Message { get; set; }        // Optional message
	}
}
