using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.DTOs;
using System;
using System.Threading.Tasks;

namespace NotificationService.Controllers
{
	[ApiController]
	[Route("api/v1/notifications")]
	public class NotificationController : ControllerBase
	{
		private readonly INotificationService _notificationService;

		public NotificationController(INotificationService notificationService)
		{
			_notificationService = notificationService;
		}

		/// <summary>
		/// Send a notification
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> SendNotification([FromBody] NotificationDTO dto)
		{
			var notification = new Notification
			{
				RecipientId = dto.RecipientId,
				Message = dto.Message,
				Type = dto.Type,
				Subject = dto.Subject
			};

			await _notificationService.SendNotificationAsync(notification);
			return Ok(notification);
		}

		/// <summary>
		/// Get all notifications for a user
		/// </summary>
		[HttpGet("{userId}")]
		public async Task<IActionResult> GetUserNotifications(string userId)
		{
			var notifications = await _notificationService.GetUserNotificationsAsync(userId);
			return Ok(notifications);
		}

		/// <summary>
		/// Mark a notification as read
		/// </summary>
		[HttpPatch("read/{notificationId}")]
		public async Task<IActionResult> MarkAsRead(Guid notificationId)
		{
			await _notificationService.MarkAsReadAsync(notificationId);
			return NoContent();
		}

		/// <summary>
		/// Get user notification settings
		/// </summary>
		[HttpGet("settings/{userId}")]
		public async Task<IActionResult> GetUserSettings(string userId)
		{
			var settings = await _notificationService.GetUserSettingsAsync(userId);
			return Ok(settings);
		}

		/// <summary>
		/// Update user notification settings
		/// </summary>
		[HttpPut("settings")]
		public async Task<IActionResult> UpdateUserSettings([FromBody] UserNotificationSetting settings)
		{
			await _notificationService.UpdateUserSettingsAsync(settings);
			return NoContent();
		}
	}
}