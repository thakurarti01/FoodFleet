using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;

namespace NotificationService.Controllers
{
    /// <summary>
    /// REST endpoints for in-app notifications.
    /// GET  /api/notifications/{userId}          — fetch all notifications for a user
    /// PATCH /api/notifications/{id}/read        — mark a notification as read
    /// Actual notification creation is event-driven via RabbitMQConsumer, not via HTTP.
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetByUser(string userId)
            => Ok(await _service.GetByUserAsync(userId));

        [Authorize]
        [HttpPatch("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            await _service.MarkAsReadAsync(notificationId);
            return NoContent();
        }
    }
}
