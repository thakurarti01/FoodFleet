using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.Data;
using NotificationService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Services
{
	public class NotificationServiceImp : INotificationService
	{
		private readonly NotificationDbContext _context;
		private readonly IHubContext<NotificationHub> _hub;
		private readonly IEmailService _emailService;

		public NotificationServiceImp(
			NotificationDbContext context,
			IHubContext<NotificationHub> hub,
			IEmailService emailService)
		{
			_context = context;
			_hub = hub;
			_emailService = emailService;
		}

		// ✅ SEND NOTIFICATION
		public async Task SendNotificationAsync(Notification notification)
		{
			// Check user settings
			var settings = await _context.UserNotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == notification.RecipientId);

			bool canSend = settings == null || IsChannelEnabled(settings, notification.Type);
			if (!canSend) return;

			// Save notification
			_context.Notifications.Add(notification);
			await _context.SaveChangesAsync();

			// Handle type
			switch (notification.Type)
			{
				case "InApp":
					await SendInApp(notification);
					break;

				case "Email":
					await SendEmail(notification);
					break;

				case "SMS":
				case "Push":
					// Future implementation
					break;
			}
		}

		// ✅ IN-APP NOTIFICATION
		private async Task SendInApp(Notification notification)
		{
			await _hub.Clients.User(notification.RecipientId)
				.SendAsync("ReceiveNotification", notification.Message);

			notification.IsSent = true;
			notification.SentAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();
		}

		// ✅ EMAIL NOTIFICATION
		private async Task SendEmail(Notification notification)
		{
			var email = await GetEmailFromRecipientId(notification.RecipientId);

			if (string.IsNullOrEmpty(email))
				return;

			try
			{
				await _emailService.SendEmailAsync(
					email,
					notification.Subject ?? "Notification",
					notification.Message
				);

				notification.IsSent = true;
				notification.SentAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Email failed: {ex.Message}");
			}
		}

		// ✅ GET USER NOTIFICATIONS
		public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
		{
			return await _context.Notifications
				.Where(n => n.RecipientId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		// ✅ MARK AS READ
		public async Task MarkAsReadAsync(Guid notificationId)
		{
			var notification = await _context.Notifications.FindAsync(notificationId);

			if (notification == null)
				return;

			notification.IsRead = true;
			await _context.SaveChangesAsync();
		}

		// ✅ GET USER SETTINGS
		public async Task<UserNotificationSetting> GetUserSettingsAsync(string userId)
		{
			var settings = await _context.UserNotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (settings != null)
				return settings;

			// Create default
			var newSettings = new UserNotificationSetting
			{
				UserId = userId,
				EmailEnabled = true,
				SMSEnabled = true,
				PushEnabled = true,
				InAppEnabled = true
			};

			_context.UserNotificationSettings.Add(newSettings);
			await _context.SaveChangesAsync();

			return newSettings;
		}

		// ✅ UPDATE SETTINGS
		public async Task UpdateUserSettingsAsync(UserNotificationSetting settings)
		{
			var existing = await _context.UserNotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == settings.UserId);

			if (existing != null)
			{
				existing.EmailEnabled = settings.EmailEnabled;
				existing.SMSEnabled = settings.SMSEnabled;
				existing.PushEnabled = settings.PushEnabled;
				existing.InAppEnabled = settings.InAppEnabled;
			}
			else
			{
				_context.UserNotificationSettings.Add(settings);
			}

			await _context.SaveChangesAsync();
		}

		// ✅ HELPER: CHANNEL CHECK
		private bool IsChannelEnabled(UserNotificationSetting settings, string type)
		{
			return type switch
			{
				"Email" => settings.EmailEnabled,
				"SMS" => settings.SMSEnabled,
				"Push" => settings.PushEnabled,
				"InApp" => settings.InAppEnabled,
				_ => true
			};
		}

		// ✅ HELPER: GET EMAIL
		private async Task<string?> GetEmailFromRecipientId(string recipientId)
		{
			var user = await _context.Users
				.FirstOrDefaultAsync(u => u.Id == recipientId);

			return user?.Email;
		}
	}
}