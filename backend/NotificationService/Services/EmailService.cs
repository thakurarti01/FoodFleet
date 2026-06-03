using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using NotificationService.Interfaces;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    /// <summary>
    /// Sends transactional emails via Gmail SMTP using MailKit.
    /// SMTP credentials are read from appsettings EmailSettings section.
    /// Used by RabbitMQConsumer to deliver welcome, order confirmation, and delivery emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string message)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailSettings["From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            
            // Check if message contains HTML tags to determine format
            var isHtml = message.TrimStart().StartsWith("<!DOCTYPE") || message.TrimStart().StartsWith("<html");
            
            email.Body = new TextPart(isHtml ? MimeKit.Text.TextFormat.Html : MimeKit.Text.TextFormat.Plain)
            {
                Text = message
            };

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                Console.WriteLine($"[EmailService] Email sent successfully to {to} | Subject: {subject}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailService] FAILED to send email to {to} | Subject: {subject} | Error: {ex.Message}");
                throw; // re-throw so caller knows it failed
            }
        }
    }
}
