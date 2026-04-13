using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PaymentService.Services
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

            _channel.QueueDeclare(queue: "order_placed_queue", durable: true, exclusive: false, autoDelete: false);
        }

        public void PublishOrderPlaced(int orderId, string customerEmail, string customerName,
            decimal totalPrice, List<string> itemNames, string paymentMethod)
        {
            var message = JsonSerializer.Serialize(new
            {
                OrderId = orderId,
                CustomerEmail = customerEmail,
                CustomerName = customerName,
                TotalPrice = totalPrice,
                ItemNames = itemNames,
                PaymentMethod = paymentMethod  // "Card" or "COD"
            });

            var body = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(exchange: "", routingKey: "order_placed_queue", basicProperties: props, body: body);
        }
    }
}
