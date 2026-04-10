using System;

namespace OrderService.Models
{
	public class Notification
	{
		public int Id { get; set; }
		public int UserId { get; set; } // Customer or Restaurant Owner
		public string Type { get; set; } // Email, Push, SignalR
		public string Message { get; set; }
		public string Status { get; set; } // Sent, Pending
		public int? RelatedOrderId { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}