namespace OrderService.DTOs
{
	public class CartItemDTO
	{
		public int MenuItemId { get; set; }
		public int Quantity { get; set; }
		public string Customizations { get; set; }
	}
}