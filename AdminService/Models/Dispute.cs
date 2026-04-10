namespace AdminService.Models
{
	public class Dispute
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public string Issue { get; set; }
		public string Status { get; set; } = "Pending";
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}