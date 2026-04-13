using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeliveryService.Models
{
	public class Delivery
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int OrderId { get; set; }

		// Foreign Key
		public int AgentId { get; set; }

		[ForeignKey("AgentId")]
		public DeliveryAgent Agent { get; set; }

		[Required]
		public string Status { get; set; } = "Assigned";
		// Assigned, Accepted, PickedUp, EnRoute, Delivered

		[Required]
		public string PickupLocation { get; set; }

		[Required]
		public string DeliveryLocation { get; set; }

		public double EstimatedTime { get; set; } // in minutes

		public DateTime? ActualDeliveryTime { get; set; }

		// Customer info for email notification on delivery
		public string? CustomerEmail { get; set; }
		public string? CustomerName { get; set; }
		public string? ItemNames { get; set; } // comma-separated item names
	}
}