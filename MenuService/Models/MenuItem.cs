namespace MenuService.Models
{
	public class MenuItem
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
		public bool IsAvailable { get; set; } = true;
		public string DietType { get; set; } // "Veg", "Non-Veg", "Vegan"

		public int CategoryId { get; set; }
		public MenuCategory? Category { get; set; }
		public ICollection<MenuItemOption> Options { get; set; } = new List<MenuItemOption>();


		//------------------------
		public decimal? DiscountPercentage { get; set; }
		public DateTime? DiscountStart { get; set; }
		public DateTime? DiscountEnd { get; set; }
	}
}
