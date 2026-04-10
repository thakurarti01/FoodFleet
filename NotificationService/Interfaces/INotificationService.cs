using NotificationService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotificationService.Interfaces
{
	public interface INotificationService
	{
		/// <summary>
		/// Send a notification to a user (Email, SMS, Push, InApp)
		/// </summary>
		Task SendNotificationAsync(Notification notification);

		/// <summary>
		/// Get all notifications for a specific user
		/// </summary>
		Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);

		/// <summary>
		/// Mark a specific notification as read
		/// </summary>
		Task MarkAsReadAsync(Guid notificationId);

		/// <summary>
		/// Get user notification settings
		/// </summary>
		Task<UserNotificationSetting> GetUserSettingsAsync(string userId);

		/// <summary>
		/// Update user notification settings
		/// </summary>
		Task UpdateUserSettingsAsync(UserNotificationSetting settings);
	}
}