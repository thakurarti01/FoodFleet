namespace NotificationService.Models
{
	public class UserNotificationSetting
	{
		public Guid Id { get; set; }
		public string UserId { get; set; }
		public bool EmailEnabled { get; set; } = true;
		public bool SMSEnabled { get; set; } = true;
		public bool PushEnabled { get; set; } = true;
		public bool InAppEnabled { get; set; } = true;
	}
}