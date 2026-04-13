namespace DeliveryService.DTOs
{
	public class DeliveryDTO
	{
		public int OrderId { get; set; }
		public string PickupLocation { get; set; }
		public string DeliveryLocation { get; set; }

		// optional: order location coordinates
		public double PickupLatitude { get; set; }
		public double PickupLongitude { get; set; }

		// customer info for delivery email notification
		public string? CustomerEmail { get; set; }
		public string? CustomerName { get; set; }
		public string? ItemNames { get; set; } // comma-separated e.g. "Burger x2, Fries x1"
	}
}