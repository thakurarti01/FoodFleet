using System.Collections.Generic;

namespace OrderService.DTOs
{
	public class CartDTO
	{
		public int CustomerId { get; set; }
		public List<CartItemDTO> Items { get; set; } = new();
	}
}