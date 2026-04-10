using System.ComponentModel.DataAnnotations;

namespace DeliveryService.Models
{
	public class DeliveryAgent
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Phone { get; set; }

		public string VehicleType { get; set; }

		public bool IsAvailable { get; set; } = true;

		// Current GPS Location
		public double CurrentLatitude { get; set; }
		public double CurrentLongitude { get; set; }

		// Navigation
		public ICollection<Delivery> Deliveries { get; set; }
	}
}