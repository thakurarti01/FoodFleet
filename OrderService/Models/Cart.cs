using System.Collections.Generic;

namespace OrderService.Models
{
	public class Cart
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }

		// Navigation property
		public List<CartItem> Items { get; set; } = new();
	}
}