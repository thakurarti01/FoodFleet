namespace PaymentService.DTOs
{
	public class RefundRequestDTO
	{
		public int PaymentId { get; set; }
		public decimal Amount { get; set; }
		public string Reason { get; set; }
	}
}
