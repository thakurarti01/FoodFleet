namespace AdminService.Models
{
	public class Refund
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public string Status { get; set; } = "Initiated";
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}