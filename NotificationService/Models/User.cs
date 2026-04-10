using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
	public class User
	{
		[Key]
		public string Id { get; set; }           // Matches RecipientId in Notification
		[Required]
		public string Name { get; set; }
		[Required]
		public string Email { get; set; }
	}
}