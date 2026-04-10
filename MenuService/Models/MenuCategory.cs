namespace MenuService.Models
{
	public class MenuCategory
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
	}
}
