using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.Data;
using NotificationService.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.Services
{
	public class NotificationService : INotificationService
	{
		private readonly NotificationDbContext _context;
		private readonly IHubContext<NotificationHub> _hub;
		private readonly IEmailService _emailService;

		public NotificationService(
			NotificationDbContext context,
			IHubContext<NotificationHub> hub,
			IEmailService emailService)
		{
			_context = context;
			_hub = hub;
			_emailService = emailService;
		}

		/// <summary>
		/// Send a notification to user, supports InApp (SignalR) and Email
		/// </summary>
		public async Task SendNotificationAsync(Notification notification)
		{
			// Check user settings
			var settings = await _context.UserNotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == notification.RecipientId);

			bool canSend = settings == null || IsChannelEnabled(settings, notification.Type);
			if (!canSend) return;

			// Save notification in DB
			_context.Notifications.Add(notification);
			await _context.SaveChangesAsync();

			// In-app push via SignalR
			if (notification.Type == "InApp")
			{
				await _hub.Clients.User(notification.RecipientId)
					.SendAsync("ReceiveNotification", notification.Message);

				notification.IsSent = true;
				notification.SentAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}

			// Email sending
			if (notification.Type == "Email")
			{
				string recipientEmail = GetEmailFromRecipientId(notification.RecipientId);
				if (!string.IsNullOrEmpty(recipientEmail))
				{
					try
					{
						await _emailService.SendEmailAsync(recipientEmail, notification.Subject, notification.Message);
						notification.IsSent = true;
						notification.SentAt = DateTime.UtcNow;
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						// Log failure (optional)
						Console.WriteLine($"Email sending failed: {ex.Message}");
					}
				}
			}

			// TODO: Add SMS or Push logic here if needed in future
		}

		/// <summary>
		/// Get all notifications for a user
		/// </summary>
		public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId)
		{
			return await _context.Notifications
				.Where(n => n.RecipientId == userId)
				.OrderByDescending(n => n.CreatedAt)
				.ToListAsync();
		}

		/// <summary>
		/// Mark a notification as read
		/// </summary>
		public async Task MarkAsReadAsync(Guid notificationId)
		{
			var notification = await _context.Notifications.FindAsync(notificationId);
			if (notification != null)
			{
				notification.IsRead = true;
				await _context.SaveChangesAsync();
			}
		}

		/// <summary>
		/// Get user notification settings, create default if not exist
		/// </summary>
		public async Task<UserNotificationSetting> GetUserSettingsAsync(string userId)
		{
			var settings = await _context.UserNotificationSettings
				.FirstOrDefaultAsync(s => s.UserId == userId);

			if (settings == null)
			{
				settings = new UserNotificationSetting
				{
					UserId = userId,
					EmailEnabled = true,
					SMSEnabled = true,
					PushEnabled = true,
					InAppEnabled = true
				};
				_context.UserNotificationSettings.Add(settings);
				await _context.SaveChangesAsync();
			}

			return settings;
		}

		/// <summary>
		/// Update user notification settings
		/// </summary>
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

		/// <summary>
		/// Helper to check if a notification type is enabled in user settings
		/// </summary>
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

		/// <summary>
		/// Map recipientId to actual email address
		/// Replace with actual lookup from Users table
		/// </summary>
		private string GetEmailFromRecipientId(string recipientId)
		{
			var user = _context.Users.FirstOrDefault(u => u.Id == recipientId);
			return user?.Email; // returns null if not found
		}
	}
}