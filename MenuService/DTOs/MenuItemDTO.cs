namespace MenuService.DTOs
{
	public class MenuItemDTO
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
		public bool IsAvailable { get; set; }
		public string DietType { get; set; }
		public int CategoryId { get; set; }
	}
}
