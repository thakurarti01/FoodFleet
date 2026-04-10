namespace AdminService.Models
{
	public class Restaurant
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string OwnerName { get; set; }
		public bool IsApproved { get; set; } = false;
		public bool IsRejected { get; set; } = false;
	}
}