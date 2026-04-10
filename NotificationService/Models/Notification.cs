namespace NotificationService.Models
{
	public class Notification
	{
		public Guid Id { get; set; }                         // Unique ID
		public string RecipientId { get; set; }              // User, Agent, or Admin ID
		public string Message { get; set; }                  // Notification content
		public string Type { get; set; }                     // "Email", "SMS", "Push", "InApp"
		public bool IsRead { get; set; } = false;           // Has the user read it
		public bool IsSent { get; set; } = false;           // Has it been successfully sent
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? SentAt { get; set; }               // When it was sent
		public string? Subject { get; set; }                // Optional, for Email
	}
}