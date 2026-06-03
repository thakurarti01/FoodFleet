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
    public class RabbitMQPublisher : IDisposable // this class connects to rabbitmq and sends(publishes) msg to a queue and uses IDisposable to clean resources
    {
        private IConnection? _connection; //connection to rabbitmq server
        private IModel? _channel; //used to send msg
        private bool _available = false; //flag(is rabbitmq working or not)

        public RabbitMQPublisher() // constructor(runs once)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // queue creation: if the queue doesn't exist, it will be created. If it already exists, this is a no-op.
                _channel.QueueDeclare("user_registered_queue", durable: false, exclusive: false, autoDelete: false); 
                //durable-false means data will lost if server restarts
                //exclusive-false means multiple apps can use the queue
                //autoDelete-false means queue will not be deleted when no longer used

                _available = true; //rabbitmq connected successfully
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
        public void PublishUserRegistered(string email, string name, string role) //this will be called after user registers
        {
            if (!_available) return;//if rabbitmq is down, do nothing
            try
            {
                //converting user data into json-string and then byte array to send to rabbitmq(as rabbitmq only accepts byte array)
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
            _channel?.Close(); //close the channel
            _connection?.Close(); //close the connection
        }
    }
}