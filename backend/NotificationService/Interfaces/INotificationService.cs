using NotificationService.Models;

namespace NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetByUserAsync(string userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task CreateAsync(string userId, string message, string type = "InApp");
    }
}
