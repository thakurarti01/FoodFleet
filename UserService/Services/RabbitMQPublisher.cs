using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace UserService.Services
{
	public class RabbitMQPublisher
	{
		public void PublishUserRegistered(string email, string name)
		{
			var factory = new ConnectionFactory()
			{
				HostName = "localhost"
			};

			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();

			// Declare queue
			channel.QueueDeclare(
				queue: "user_registered_queue",
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			// Create message
			var message = JsonSerializer.Serialize(new
			{
				Email = email,
				UserName = name
			});

			var body = Encoding.UTF8.GetBytes(message);

			// Publish message
			channel.BasicPublish(
				exchange: "",
				routingKey: "user_registered_queue",
				basicProperties: null,
				body: body
			);
		}
	}
}