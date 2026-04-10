using System;
using System.Collections.Generic;

namespace OrderService.Models
{
	public class Order
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public int RestaurantId { get; set; }
		public int DeliveryAddressId { get; set; }
		public decimal TotalPrice { get; set; }
		public decimal Taxes { get; set; }
		public decimal DeliveryFee { get; set; }
		public decimal Discounts { get; set; }
		public string Status { get; set; } // Placed, Confirmed, Preparing, Ready, PickedUp, Delivered
		public bool IsCancelled { get; set; }
		public DateTime PlacedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }

		// Navigation property
		public List<OrderItem> Items { get; set; } = new();
	}
}