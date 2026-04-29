using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Exceptions;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Services
{
    /// <summary>
    /// Manages in-app notification records in the database.
    /// Supports fetching a user's notifications, marking them as read,
    /// and creating new notification entries (called by RabbitMQConsumer on each event).
    /// </summary>
    public class NotificationServiceImp : INotificationService
    {
        private readonly NotificationDbContext _context;

        public NotificationServiceImp(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(string userId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve notifications: {ex.Message}", ex);
            }
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var n = await _context.Notifications.FindAsync(notificationId)
                    ?? throw new NotificationNotFoundException(notificationId);

                n.IsRead = true;
                await _context.SaveChangesAsync();
            }
            catch (NotificationNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to mark notification as read: {ex.Message}", ex);
            }
        }

        public async Task CreateAsync(string userId, string message, string type = "InApp")
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Message = message,
                    Type = type,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create notification: {ex.Message}", ex);
            }
        }
    }
}
