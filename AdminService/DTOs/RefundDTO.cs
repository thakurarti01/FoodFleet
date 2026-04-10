namespace AdminService.DTOs
{
	public class RefundDTO
	{
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public string Reason { get; set; }
	}
}