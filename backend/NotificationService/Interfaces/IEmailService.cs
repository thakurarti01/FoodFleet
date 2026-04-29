using System.Threading.Tasks;

namespace NotificationService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string message);
    }
}
