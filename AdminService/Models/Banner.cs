namespace AdminService.Models
{
	public class Banner
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string ImageUrl { get; set; }
		public bool IsActive { get; set; } = true;
	}
}