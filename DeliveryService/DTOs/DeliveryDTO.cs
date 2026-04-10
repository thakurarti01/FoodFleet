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
	}
}