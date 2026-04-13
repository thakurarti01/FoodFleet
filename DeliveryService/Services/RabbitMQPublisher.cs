using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace DeliveryService.Services
{
    public class RabbitMQPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMQPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "order_delivered_queue", durable: true, exclusive: false, autoDelete: false);
        }

        public void PublishOrderDelivered(int orderId, string customerEmail, string customerName, List<string> itemNames)
        {
            var message = JsonSerializer.Serialize(new
            {
                OrderId = orderId,
                CustomerEmail = customerEmail,
                CustomerName = customerName,
                ItemNames = itemNames
            });

            var body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: "order_delivered_queue", basicProperties: props, body: body);
        }
    }
}
