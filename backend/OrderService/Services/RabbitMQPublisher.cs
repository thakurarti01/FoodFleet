using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// Publishes order lifecycle events to RabbitMQ queues.
    /// NotificationService consumes these to send emails to customers.
    /// Registered as Singleton — connection is reused across requests.
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

                // Declare queues — must match NotificationService consumer declarations
                _channel.QueueDeclare("order_placed_queue",    durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare("order_delivered_queue", durable: true, exclusive: false, autoDelete: false);
                _channel.QueueDeclare("otp_generated_queue",   durable: true, exclusive: false, autoDelete: false);

                _available = true;
                Console.WriteLine("OrderService RabbitMQ publisher connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OrderService RabbitMQ unavailable (notifications disabled): {ex.Message}");
            }
        }

        /// <summary>
        /// Fires when a customer places an order.
        /// Triggers an order confirmation email via NotificationService.
        /// </summary>
        public void PublishOrderPlaced(int orderId, string customerEmail, string customerName,
            decimal totalPrice, List<string> itemNames, string paymentMethod)
        {
            if (!_available)
            {
                Console.WriteLine($"[OrderService] RabbitMQ unavailable, cannot publish order_placed for order #{orderId}");
                return;
            }
            try
            {
                Console.WriteLine($"[OrderService] Publishing to order_placed_queue: OrderId={orderId}, Email={customerEmail}");
                Publish("order_placed_queue", new
                {
                    OrderId = orderId,
                    CustomerEmail = customerEmail,
                    CustomerName = customerName,
                    TotalPrice = totalPrice,
                    ItemNames = itemNames,
                    PaymentMethod = paymentMethod
                });
                Console.WriteLine($"[OrderService] Successfully published order_placed for order #{orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish order_placed: {ex.Message}");
            }
        }

        /// <summary>
        /// Fires when delivery status is set to Delivered.
        /// Triggers a delivery confirmation email via NotificationService.
        /// </summary>
        public void PublishOrderDelivered(int orderId, string customerEmail, string customerName,
            List<string> itemNames)
        {
            if (!_available) return;
            try
            {
                Publish("order_delivered_queue", new
                {
                    OrderId = orderId,
                    CustomerEmail = customerEmail,
                    CustomerName = customerName,
                    ItemNames = itemNames
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish order_delivered: {ex.Message}");
            }
        }

        /// <summary>
        /// Fires when an OTP is generated for delivery verification.
        /// Triggers an OTP email via NotificationService.
        /// </summary>
        public void PublishOTPGenerated(int orderId, string customerEmail, string customerName, string otp)
        {
            if (!_available)
            {
                Console.WriteLine($"[OrderService] RabbitMQ unavailable, cannot publish OTP for order #{orderId}");
                return;
            }
            try
            {
                Console.WriteLine($"[OrderService] Publishing to otp_generated_queue: OrderId={orderId}, Email={customerEmail}");
                Publish("otp_generated_queue", new
                {
                    OrderId = orderId,
                    CustomerEmail = customerEmail,
                    CustomerName = customerName,
                    OTP = otp
                });
                Console.WriteLine($"[OrderService] Successfully published OTP for order #{orderId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish OTP: {ex.Message}");
            }
        }

        private void Publish(string queue, object payload)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
            var props = _channel!.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: props, body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
