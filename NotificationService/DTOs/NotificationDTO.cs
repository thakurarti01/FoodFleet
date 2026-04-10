namespace NotificationService.DTOs
{
	public class NotificationDTO
	{
		public string RecipientId { get; set; }
		public string Message { get; set; }
		public string Type { get; set; }     // Email, SMS, InApp, Push
		public string? Subject { get; set; } // Optional for Email
	}
}