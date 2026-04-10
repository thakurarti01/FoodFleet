using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public interface IEmailService
{
	Task SendEmailAsync(string to, string subject, string message);
}

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
		email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
		{
			Text = message
		};

		using var smtp = new SmtpClient();
		await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
		await smtp.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
		await smtp.SendAsync(email);
		await smtp.DisconnectAsync(true);
	}
}