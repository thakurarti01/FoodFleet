namespace OrderService.DTOs
{
	public class OrderItemDTO
	{
		public int MenuItemId { get; set; }
		public int Quantity { get; set; }
		public string Customizations { get; set; }
		public decimal Price { get; set; }
	}
}