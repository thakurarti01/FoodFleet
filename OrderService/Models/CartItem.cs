namespace OrderService.Models
{
	public class CartItem
	{
		public int Id { get; set; }
		public int CartId { get; set; }
		public int MenuItemId { get; set; }
		public int Quantity { get; set; }
		public string Customizations { get; set; }

		// Navigation property
		public Cart Cart { get; set; }
	}
}