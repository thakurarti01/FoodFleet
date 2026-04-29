using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace UserService.Services
{
    /// <summary>
    /// Publishes user lifecycle events to RabbitMQ.
    /// Registered as Singleton — connection is reused across requests.
    /// NotificationService consumes these to send welcome emails on new registrations.
    /// </summary>
    public class RabbitMQPublisher : IDisposable
    {
        private IConnection? _connection;
        private IModel? _channel;
        private bool _available = false;

        public RabbitMQPublisher()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // durable: false — matches NotificationService consumer declaration
                _channel.QueueDeclare("user_registered_queue", durable: false, exclusive: false, autoDelete: false);

                _available = true;
                Console.WriteLine("UserService RabbitMQ publisher connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserService RabbitMQ unavailable (notifications disabled): {ex.Message}");
            }
        }

        /// <summary>
        /// Fires once when a brand-new user registers.
        /// NotificationService sends a one-time welcome email to the new user.
        /// </summary>
        public void PublishUserRegistered(string email, string name, string role)
        {
            if (!_available) return;
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
                {
                    Email = email,
                    UserName = name,
                    Role = role
                }));
                _channel!.BasicPublish(exchange: "", routingKey: "user_registered_queue", basicProperties: null, body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish user_registered: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}