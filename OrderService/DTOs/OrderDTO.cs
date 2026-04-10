using System;
using System.Collections.Generic;

namespace OrderService.DTOs
{
	public class OrderDTO
	{
		// Optional: used when returning orders
		public int Id { get; set; }

		// Client-settable
		public int CustomerId { get; set; }
		public int RestaurantId { get; set; }
		public int DeliveryAddressId { get; set; }
		public List<OrderItemDTO> Items { get; set; } = new();
		public decimal DeliveryFee { get; set; }
		public decimal Discounts { get; set; }
		public decimal Taxes { get; set; }

		// Calculated / returned only
		public decimal TotalPrice { get; set; }  // Calculated in service
		public string? Status { get; set; }       // Returned only
		public bool IsCancelled { get; set; }    // Returned only
		public DateTime? PlacedAt { get; set; }   // Returned only
		public DateTime? UpdatedAt { get; set; } // Returned only
	}
}