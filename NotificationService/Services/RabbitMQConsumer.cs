using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Interfaces;

namespace NotificationService.Services
{
    public class RabbitMQConsumer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMQConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void StartListening()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            // Declare all queues
            channel.QueueDeclare(queue: "user_registered_queue", durable: false, exclusive: false, autoDelete: false);
            channel.QueueDeclare(queue: "order_placed_queue", durable: true, exclusive: false, autoDelete: false);
            channel.QueueDeclare(queue: "order_delivered_queue", durable: true, exclusive: false, autoDelete: false);

            ListenToQueue(channel, "user_registered_queue", HandleUserRegistered);
            ListenToQueue(channel, "order_placed_queue", HandleOrderPlaced);
            ListenToQueue(channel, "order_delivered_queue", HandleOrderDelivered);

            Console.WriteLine("Listening for messages on all queues...");
        }

        private void ListenToQueue(IModel channel, string queue, Func<string, Task> handler)
        {
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    await handler(message);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing [{queue}]: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        }

        private async Task HandleUserRegistered(string message)
        {
            var data = JsonSerializer.Deserialize<UserRegisteredEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await emailService.SendEmailAsync(
                data.Email,
                "Welcome to FoodFleet!",
                $"Hello {data.UserName},\n\nYou have successfully registered on FoodFleet. Welcome aboard!"
            );
        }

        private async Task HandleOrderPlaced(string message)
        {
            var data = JsonSerializer.Deserialize<OrderPlacedEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var itemList = string.Join("\n", data.ItemNames.Select(i => $"  - {i}"));
            var isCOD = data.PaymentMethod?.ToUpper() == "COD";

            var paymentLine = isCOD
                ? $"Payment Method: Cash on Delivery (COD)\nAmount to pay on delivery: {data.TotalPrice:C}"
                : $"Payment Method: Card\nAmount Paid: {data.TotalPrice:C}";

            var subject = isCOD
                ? $"Order Confirmed (COD) - #{data.OrderId}"
                : $"Order Confirmed - #{data.OrderId}";

            await emailService.SendEmailAsync(
                data.CustomerEmail,
                subject,
                $"Hello {data.CustomerName},\n\n" +
                $"Your order has been placed successfully!\n\n" +
                $"Order ID: #{data.OrderId}\n" +
                $"Items:\n{itemList}\n\n" +
                $"{paymentLine}\n\n" +
                (isCOD ? "Please keep the exact amount ready at the time of delivery.\n\n" : "") +
                $"We'll notify you once your order is delivered.\n\nThank you for ordering with FoodFleet!"
            );
        }

        private async Task HandleOrderDelivered(string message)
        {
            var data = JsonSerializer.Deserialize<OrderDeliveredEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var itemList = string.Join("\n", data.ItemNames.Select(i => $"  - {i}"));

            await emailService.SendEmailAsync(
                data.CustomerEmail,
                $"Order Delivered - #{data.OrderId}",
                $"Hello {data.CustomerName},\n\n" +
                $"Great news! Your order has been delivered.\n\n" +
                $"Order ID: #{data.OrderId}\n" +
                $"Items Delivered:\n{itemList}\n\n" +
                $"We hope you enjoy your meal! Thank you for choosing FoodFleet."
            );
        }

        // Event models
        public class UserRegisteredEvent
        {
            public string Email { get; set; } = "";
            public string UserName { get; set; } = "";
        }

        public class OrderPlacedEvent
        {
            public int OrderId { get; set; }
            public string CustomerEmail { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public decimal TotalPrice { get; set; }
            public List<string> ItemNames { get; set; } = new();
            public string PaymentMethod { get; set; } = ""; // "Card" or "COD"
        }

        public class OrderDeliveredEvent
        {
            public int OrderId { get; set; }
            public string CustomerEmail { get; set; } = "";
            public string CustomerName { get; set; } = "";
            public List<string> ItemNames { get; set; } = new();
        }
    }
}
