using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Interfaces;

namespace NotificationService.Services
{
    /// <summary>
    /// Background RabbitMQ consumer that listens on three queues:
    ///   - user_registered_queue  → sends a one-time welcome email on new registration
    ///   - order_placed_queue     → sends an order confirmation email when a customer places an order
    ///   - order_delivered_queue  → sends a delivery confirmation email when an order is delivered
    /// Each event also saves an in-app notification record to the database via INotificationService.
    /// Registered as Singleton; StartListening() is called once at app startup.
    /// </summary>
    public class RabbitMQConsumer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMQConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void StartListening()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                var connection = factory.CreateConnection();
                var channel = connection.CreateModel();

                // Queue declarations must match publisher declarations exactly
                channel.QueueDeclare(queue: "user_registered_queue", durable: false, exclusive: false, autoDelete: false);
                channel.QueueDeclare(queue: "order_placed_queue",    durable: true,  exclusive: false, autoDelete: false);
                channel.QueueDeclare(queue: "order_delivered_queue", durable: true,  exclusive: false, autoDelete: false);
                channel.QueueDeclare(queue: "otp_generated_queue",   durable: true,  exclusive: false, autoDelete: false);

                ListenToQueue(channel, "user_registered_queue", HandleUserRegistered);
                ListenToQueue(channel, "order_placed_queue",    HandleOrderPlaced);
                ListenToQueue(channel, "order_delivered_queue", HandleOrderDelivered);
                ListenToQueue(channel, "otp_generated_queue",   HandleOTPGenerated);

                Console.WriteLine("RabbitMQ consumer started — listening on 4 queues.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ unavailable, notifications disabled: {ex.Message}");
            }
        }

        private void ListenToQueue(IModel channel, string queue, Func<string, Task> handler)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (_, ea) =>
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
                    channel.BasicNack(ea.DeliveryTag, false, requeue: false);
                }
            };
            channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        }

        /// <summary>
        /// Triggered once when a new user registers.
        /// Sends a role-specific welcome email and saves an in-app notification.
        /// </summary>
        private async Task HandleUserRegistered(string message)
        {
            var data = JsonSerializer.Deserialize<UserRegisteredEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            string emailBody = data.Role switch
            {
                "Customer" => $"Hello {data.UserName},\n\nWelcome to FoodFleet! 🍽️\n\nYour account has been created successfully. Start exploring amazing restaurants near you and place your first order today!\n\nHappy ordering!\nThe FoodFleet Team",
                "DeliveryAgent" => $"Hello {data.UserName},\n\nWelcome to the FoodFleet Delivery Team! 🚴\n\nYour delivery agent account is now active. Start your journey with us and help deliver happiness to our customers!\n\nLog in to view available deliveries and begin earning.\n\nBest regards,\nThe FoodFleet Team",
                "Owner" => $"Hello {data.UserName},\n\nWelcome to FoodFleet! 🏪\n\nYour restaurant owner account has been created. Register your restaurant with us and reach thousands of hungry customers!\n\nLog in to add your restaurant and start receiving orders.\n\nLooking forward to partnering with you!\nThe FoodFleet Team",
                _ => $"Hello {data.UserName},\n\nYou have successfully registered on FoodFleet. Welcome aboard!\n\nStart exploring restaurants and place your first order today."
            };

            await emailService.SendEmailAsync(
                data.Email,
                "Welcome to FoodFleet!",
                emailBody);

            await notifService.CreateAsync(data.Email,
                $"Welcome to FoodFleet, {data.UserName}! Your account has been created successfully.", "InApp");
        }

        /// <summary>
        /// Triggered when a customer places an order.
        /// Sends an order confirmation email and saves an in-app notification.
        /// </summary>
        private async Task HandleOrderPlaced(string message)
        {
            Console.WriteLine($"[NotificationService] Received order_placed event: {message}");
            var data = JsonSerializer.Deserialize<OrderPlacedEvent>(message)!;
            Console.WriteLine($"[NotificationService] Processing order #{data.OrderId} for {data.CustomerEmail}");
            
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var isCOD = data.PaymentMethod?.ToUpper() == "COD";
            
            // Build items HTML
            var itemsHtml = string.Join("", data.ItemNames.Select(item => 
                $"<tr><td style='padding: 12px; border-bottom: 1px solid #eee; font-size: 14px;'>{item}</td></tr>"));
            
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #7b1fa2, #9c27b0); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .invoice-box {{ background: #f9f9f9; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .invoice-header {{ border-bottom: 2px solid #7b1fa2; padding-bottom: 15px; margin-bottom: 15px; }}
        .items-table {{ width: 100%; border-collapse: collapse; margin: 15px 0; background: white; border-radius: 8px; overflow: hidden; }}
        .total-row {{ background: #f3e5f5; font-weight: bold; font-size: 18px; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; font-size: 14px; }}
        .status-badge {{ display: inline-block; padding: 6px 16px; border-radius: 20px; background: #4caf50; color: white; font-size: 12px; font-weight: 600; margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; background: #f9f9f9; }}
        .highlight-box {{ background: #e8f5e9; border-left: 4px solid #4caf50; padding: 15px; border-radius: 4px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0; font-size: 32px;'>🍽️ FoodFleet</h1>
            <p style='margin: 10px 0 0 0; font-size: 16px;'>Order Confirmation</p>
        </div>
        <div class='content'>
            <h2 style='color: #7b1fa2; margin-bottom: 10px;'>Thank you for your order!</h2>
            <p style='font-size: 14px; color: #666;'>Hi {data.CustomerName},</p>
            <p style='font-size: 14px; color: #666;'>Your order has been received and is being prepared with care.</p>
            
            <span class='status-badge'>✓ Order Confirmed</span>
            
            <div class='invoice-box'>
                <div class='invoice-header'>
                    <h3 style='margin: 0; color: #7b1fa2;'>📋 Invoice</h3>
                </div>
                
                <div style='margin-bottom: 15px;'>
                    <div class='info-row'>
                        <span style='color: #666;'>Order Number:</span>
                        <strong style='color: #7b1fa2;'>#{data.OrderId}</strong>
                    </div>
                    <div class='info-row'>
                        <span style='color: #666;'>Order Date:</span>
                        <strong>{DateTime.UtcNow:dd MMM yyyy, hh:mm tt}</strong>
                    </div>
                    <div class='info-row'>
                        <span style='color: #666;'>Payment Method:</span>
                        <strong>{(isCOD ? "Cash on Delivery" : "Card Payment")}</strong>
                    </div>
                    <div class='info-row'>
                        <span style='color: #666;'>Payment Status:</span>
                        <strong style='color: {(isCOD ? "#ff9800" : "#4caf50")};'>{(isCOD ? "Pending (COD)" : "Paid")}</strong>
                    </div>
                </div>
                
                <h4 style='margin: 20px 0 10px 0; color: #333; font-size: 14px;'>Items Ordered:</h4>
                <table class='items-table'>
                    {itemsHtml}
                    <tr class='total-row'>
                        <td style='padding: 15px; text-align: right; color: #7b1fa2;'>
                            Total Amount: ₹{data.TotalPrice:F2}
                        </td>
                    </tr>
                </table>
                
                {(isCOD ? "<p style='font-size: 13px; color: #666; margin-top: 10px;'><strong>Note:</strong> Please keep ₹" + data.TotalPrice.ToString("F2") + " ready for the delivery agent.</p>" : "")}
            </div>
            
            <div class='highlight-box'>
                <strong style='color: #2e7d32; font-size: 15px;'>✓ What's Next?</strong>
                <p style='margin: 10px 0 0 0; color: #1b5e20; font-size: 14px; line-height: 1.6;'>
                    • Your order is being prepared by the restaurant<br>
                    • You'll receive an OTP via email when a delivery agent is assigned<br>
                    • Share the OTP with the delivery agent at your doorstep<br>
                    • Enjoy your delicious meal!
                </p>
            </div>
            
            <p style='color: #666; font-size: 13px; margin-top: 20px; text-align: center;'>
                Need help? Contact us at <a href='mailto:support@foodfleet.com' style='color: #7b1fa2;'>support@foodfleet.com</a>
            </p>
        </div>
        <div class='footer'>
            <p style='margin: 5px 0;'>© 2026 FoodFleet. All rights reserved.</p>
            <p style='margin: 5px 0;'>This is an automated email. Please do not reply.</p>
            <p style='margin: 5px 0; font-size: 11px;'>FoodFleet Inc. | Delivering Happiness</p>
        </div>
    </div>
</body>
</html>";

            Console.WriteLine($"[NotificationService] Sending order confirmation email to {data.CustomerEmail}");
            await emailService.SendEmailAsync(
                data.CustomerEmail,
                $"Order Confirmed #{data.OrderId} - FoodFleet",
                emailBody);
            Console.WriteLine($"[NotificationService] Email sent successfully for order #{data.OrderId}");

            await notifService.CreateAsync(data.CustomerEmail,
                $"Your order #{data.OrderId} has been placed successfully. Total: ₹{data.TotalPrice:F2}", "InApp");
            Console.WriteLine($"[NotificationService] In-app notification created for order #{data.OrderId}");
        }

        /// <summary>
        /// Triggered when an order's delivery status is set to Delivered.
        /// Sends a delivery confirmation email and saves an in-app notification.
        /// </summary>
        private async Task HandleOrderDelivered(string message)
        {
            var data = JsonSerializer.Deserialize<OrderDeliveredEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notifService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            // Build items HTML
            var itemsList = string.Join("", data.ItemNames.Select(item => 
                $"<li style='padding: 8px 0; border-bottom: 1px solid #eee;'>{item}</li>"));
            
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #4caf50, #66bb6a); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .success-box {{ background: #e8f5e9; border: 2px solid #4caf50; border-radius: 10px; padding: 25px; text-align: center; margin: 20px 0; }}
        .items-box {{ background: #f9f9f9; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .rating-box {{ background: #fff3e0; border-left: 4px solid #ff9800; padding: 15px; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; background: #f9f9f9; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0; font-size: 48px;'>✓</h1>
            <h2 style='margin: 10px 0 0 0; font-size: 24px;'>Delivered!</h2>
            <p style='margin: 5px 0 0 0; font-size: 14px;'>Order #{data.OrderId}</p>
        </div>
        <div class='content'>
            <div class='success-box'>
                <h2 style='color: #2e7d32; margin: 0 0 10px 0; font-size: 22px;'>🎉 Enjoy your meal!</h2>
                <p style='margin: 0; color: #1b5e20; font-size: 15px;'>Your order has been successfully delivered</p>
            </div>
            
            <h3 style='color: #333; margin-bottom: 5px;'>Hi {data.CustomerName},</h3>
            <p style='color: #666; font-size: 14px; line-height: 1.6;'>
                We hope you enjoy your delicious meal! Your order has been delivered to your doorstep.
            </p>
            
            <div class='items-box'>
                <h4 style='color: #333; margin: 0 0 15px 0; font-size: 15px;'>📦 Items Delivered:</h4>
                <ul style='list-style: none; padding: 0; margin: 0;'>
                    {itemsList}
                </ul>
            </div>
            
            <div class='rating-box'>
                <strong style='color: #e65100; font-size: 15px;'>⭐ Rate Your Experience</strong>
                <p style='margin: 10px 0 0 0; color: #ef6c00; font-size: 14px; line-height: 1.6;'>
                    Help us improve! Please take a moment to rate your food quality and delivery experience. 
                    Your feedback helps us serve you better.
                </p>
            </div>
            
            <p style='color: #666; font-size: 13px; text-align: center; margin-top: 25px;'>
                Thank you for choosing FoodFleet! We look forward to serving you again.
            </p>
        </div>
        <div class='footer'>
            <p style='margin: 5px 0;'>© 2026 FoodFleet. All rights reserved.</p>
            <p style='margin: 5px 0;'>Questions? Contact <a href='mailto:support@foodfleet.com' style='color: #7b1fa2;'>support@foodfleet.com</a></p>
            <p style='margin: 5px 0; font-size: 11px;'>FoodFleet Inc. | Delivering Happiness</p>
        </div>
    </div>
</body>
</html>";

            await emailService.SendEmailAsync(
                data.CustomerEmail,
                $"Order Delivered #{data.OrderId} - FoodFleet",
                emailBody);

            await notifService.CreateAsync(data.CustomerEmail,
                $"Your order #{data.OrderId} has been delivered. Enjoy your meal!", "InApp");
        }

        /// <summary>
        /// Triggered when an OTP is generated for delivery verification.
        /// Sends an OTP email to the customer.
        /// </summary>
        private async Task HandleOTPGenerated(string message)
        {
            var data = JsonSerializer.Deserialize<OTPGeneratedEvent>(message)!;
            using var scope = _scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 0; background: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #7b1fa2, #9c27b0); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .otp-box {{ background: #f3e5f5; border: 2px dashed #7b1fa2; border-radius: 10px; padding: 20px; text-align: center; margin: 20px 0; }}
        .otp {{ font-size: 48px; font-weight: bold; color: #7b1fa2; letter-spacing: 8px; }}
        .warning {{ background: #fff3e0; border-left: 4px solid #ff9800; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; background: #f9f9f9; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0;'>🍽️ FoodFleet</h1>
            <p style='margin: 10px 0 0 0;'>Delivery OTP</p>
        </div>
        <div class='content'>
            <h2>Hello {data.CustomerName}!</h2>
            <p>Your order is out for delivery. Please share this OTP with the delivery agent to confirm delivery:</p>
            
            <div class='otp-box'>
                <p style='margin: 0 0 10px 0; font-size: 14px; color: #666;'>Your Delivery OTP</p>
                <div class='otp'>{data.OTP}</div>
                <p style='margin: 10px 0 0 0; font-size: 12px; color: #666;'>Valid for 30 minutes</p>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Important:</strong>
                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                    <li>Only share this OTP with the delivery agent at your doorstep</li>
                    <li>Do not share this OTP with anyone over phone or message</li>
                    <li>FoodFleet will never ask for your OTP</li>
                </ul>
            </div>
            
            <p style='color: #666; font-size: 13px;'>Order Number: <strong>#{data.OrderId}</strong></p>
        </div>
        <div class='footer'>
            <p>© 2026 FoodFleet. All rights reserved.</p>
            <p>This is an automated email. Please do not reply.</p>
        </div>
    </div>
</body>
</html>";

            await emailService.SendEmailAsync(
                data.CustomerEmail,
                $"Delivery OTP for Order #{data.OrderId}",
                emailBody);

            Console.WriteLine($"[NotificationService] OTP email sent to {data.CustomerEmail} for order {data.OrderId}");
        }

        // Event payload models — must match publisher serialization
        public class UserRegisteredEvent
        {
            public string Email    { get; set; } = "";
            public string UserName { get; set; } = "";
            public string Role     { get; set; } = "Customer";
        }

        public class OrderPlacedEvent
        {
            public int           OrderId       { get; set; }
            public string        CustomerEmail { get; set; } = "";
            public string        CustomerName  { get; set; } = "";
            public decimal       TotalPrice    { get; set; }
            public List<string>  ItemNames     { get; set; } = new();
            public string        PaymentMethod { get; set; } = "";
        }

        public class OrderDeliveredEvent
        {
            public int           OrderId       { get; set; }
            public string        CustomerEmail { get; set; } = "";
            public string        CustomerName  { get; set; } = "";
            public List<string>  ItemNames     { get; set; } = new();
        }

        public class OTPGeneratedEvent
        {
            public int    OrderId       { get; set; }
            public string CustomerEmail { get; set; } = "";
            public string CustomerName  { get; set; } = "";
            public string OTP           { get; set; } = "";
        }
    }
}
