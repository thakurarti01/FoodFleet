namespace OrderService.Models
{
	public class OrderItem
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public int MenuItemId { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public string Customizations { get; set; } // JSON string for options

		// Navigation property
		public Order Order { get; set; }
	}
}