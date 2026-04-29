using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Exceptions;
using OrderService.Interfaces;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// Core business logic for order management.
    /// Handles placing orders, status updates, cancellations, and delivery tracking.
    /// On order placement, auto-assigns a free delivery agent (active + not currently out on a delivery).
    /// Publishes RabbitMQ events for order placed and order delivered
    /// so NotificationService can send emails to customers.
    /// </summary>
    public class OrderServiceImp : IOrderService
    {
        private readonly OrderDbContext _context;
        private readonly RabbitMQPublisher _publisher;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderServiceImp(OrderDbContext context, RabbitMQPublisher publisher, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _publisher = publisher;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Order> PlaceOrderAsync(PlaceOrderDto dto)
        {
            try
            {
                if (dto.Items == null || dto.Items.Count == 0)
                    throw new EmptyOrderException();

                // Fetch restaurant name from RestaurantService
                string? restaurantName = null;
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var response = await httpClient.GetAsync($"http://localhost:5214/api/restaurants/{dto.RestaurantId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var restaurantData = JsonSerializer.Deserialize<JsonElement>(json);
                        restaurantName = restaurantData.GetProperty("name").GetString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[OrderService] Failed to fetch restaurant name: {ex.Message}");
                }

                var order = new Order
                {
                    UserId = dto.UserId,
                    RestaurantId = dto.RestaurantId,
                    DeliveryAddress = dto.DeliveryAddress,
                    CustomerEmail = dto.CustomerEmail,
                    CustomerName = dto.CustomerName,
                    RestaurantName = restaurantName,
                    Status = "Placed",
                    DeliveryStatus = "Pending",
                    PaymentStatus = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    Items = dto.Items.Select(i => new OrderItem
                    {
                        MenuItemId = i.MenuItemId,
                        MenuItemName = i.MenuItemName,
                        Price = i.Price,
                        Quantity = i.Quantity,
                        Customizations = i.Customizations
                    }).ToList()
                };

                order.TotalAmount = order.Items.Sum(i => i.Price * i.Quantity);

                // Auto-assign a free delivery agent before saving
                var assignedAgentId = await TryAutoAssignAgentAsync();
                if (assignedAgentId.HasValue)
                {
                    order.DeliveryAgentId = assignedAgentId;
                    order.DeliveryStatus = "Assigned";
                    
                    // Generate OTP for delivery verification
                    order.DeliveryOTP = GenerateOTP();
                    order.OTPGeneratedAt = DateTime.UtcNow;
                    
                    Console.WriteLine($"[OrderService] Auto-assigned agent {assignedAgentId} to new order.");
                }
                else
                {
                    Console.WriteLine("[OrderService] No available delivery agent found. Order will be unassigned.");
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Send OTP to customer if agent was assigned
                if (assignedAgentId.HasValue && !string.IsNullOrEmpty(dto.CustomerEmail))
                {
                    _publisher.PublishOTPGenerated(
                        order.Id,
                        dto.CustomerEmail,
                        dto.CustomerName ?? "Customer",
                        order.DeliveryOTP!
                    );
                }

                // Publish event so NotificationService sends order confirmation email
                if (!string.IsNullOrEmpty(dto.CustomerEmail))
                {
                    _publisher.PublishOrderPlaced(
                        order.Id,
                        dto.CustomerEmail,
                        dto.CustomerName ?? "Customer",
                        order.TotalAmount,
                        order.Items.Select(i => $"{i.MenuItemName} x{i.Quantity}").ToList(),
                        "COD"
                    );
                }

                return order;
            }
            catch (EmptyOrderException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to place order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Fetches all active delivery agents from UserService, then filters out
        /// any who already have an order with DeliveryStatus Assigned or PickedUp.
        /// Returns a randomly selected free agent's ID, or null if none available.
        /// </summary>
        private async Task<Guid?> TryAutoAssignAgentAsync()
        {
            try
            {
                // Get all active delivery agents from UserService
                var client = _httpClientFactory.CreateClient("UserService");
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/delivery-agents/internal");
                request.Headers.Add("X-Service-Key", "foodfleet-internal-2024");
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                var agents = JsonSerializer.Deserialize<List<AgentDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (agents == null || agents.Count == 0) return null;

                // Only consider active agents
                var activeAgents = agents.Where(a => a.IsActive).ToList();
                if (activeAgents.Count == 0) return null;

                // Find agents who are currently busy (have an active delivery)
                // Exclude cancelled orders
                var busyAgentIds = await _context.Orders
                    .Where(o => o.DeliveryAgentId != null
                             && (o.DeliveryStatus == "Assigned" || o.DeliveryStatus == "PickedUp")
                             && o.Status != "Cancelled")
                    .Select(o => o.DeliveryAgentId!.Value)
                    .Distinct()
                    .ToListAsync();

                // Free = active and not currently out on a delivery
                var freeAgents = activeAgents
                    .Where(a => !busyAgentIds.Contains(a.UserId))
                    .ToList();

                if (freeAgents.Count == 0) return null;

                // Pick one randomly
                var chosen = freeAgents[Random.Shared.Next(freeAgents.Count)];
                return chosen.UserId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] Auto-assign failed (non-critical): {ex.Message}");
                return null;
            }
        }

        private record AgentDto(Guid UserId, string FullName, string Email, bool IsActive);

        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<Order?> GetByIdAsync(int orderId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve order #{orderId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetByUserAsync(Guid userId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve orders for user: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetByRestaurantAsync(Guid restaurantId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.RestaurantId == restaurantId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve orders for restaurant: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetByAgentAsync(Guid agentId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.DeliveryAgentId == agentId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve orders for agent: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetUnassignedAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.DeliveryAgentId == null
                             && o.Status != "Cancelled"
                             && o.Status != "Delivered")
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve unassigned orders: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Order>> GetActiveDeliveriesAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Items)
                    .Where(o => o.DeliveryStatus == "Assigned" || o.DeliveryStatus == "PickedUp")
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve active deliveries: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateStatusAsync(int orderId, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId)
                    ?? throw new OrderNotFoundException(orderId);

                if (order.Status == "Cancelled")
                    throw new OrderAlreadyCancelledException(orderId);

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (OrderNotFoundException) { throw; }
            catch (OrderAlreadyCancelledException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update order status: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelAsync(int orderId, string reason)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId)
                    ?? throw new OrderNotFoundException(orderId);

                if (order.Status == "Cancelled")
                    throw new OrderAlreadyCancelledException(orderId);

                if (order.Status == "Delivered")
                    throw new OrderNotCancellableException(orderId, order.Status);

                order.Status = "Cancelled";
                order.CancellationReason = reason;
                order.UpdatedAt = DateTime.UtcNow;
                
                // Free up the delivery agent if one was assigned
                if (order.DeliveryAgentId.HasValue)
                {
                    order.DeliveryStatus = "Cancelled";
                    Console.WriteLine($"[OrderService] Order #{orderId} cancelled - freeing up delivery agent {order.DeliveryAgentId}");
                }
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (OrderNotFoundException) { throw; }
            catch (OrderAlreadyCancelledException) { throw; }
            catch (OrderNotCancellableException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to cancel order: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssignAgentAsync(int orderId, Guid agentId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId)
                    ?? throw new OrderNotFoundException(orderId);

                order.DeliveryAgentId = agentId;
                order.DeliveryStatus = "Assigned";
                order.UpdatedAt = DateTime.UtcNow;
                
                // Generate OTP for delivery verification
                order.DeliveryOTP = GenerateOTP();
                order.OTPGeneratedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                // Send OTP to customer via email
                if (!string.IsNullOrEmpty(order.CustomerEmail))
                {
                    _publisher.PublishOTPGenerated(
                        order.Id,
                        order.CustomerEmail,
                        order.CustomerName ?? "Customer",
                        order.DeliveryOTP
                    );
                }
                
                return true;
            }
            catch (OrderNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to assign agent: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateDeliveryStatusAsync(int orderId, string deliveryStatus)
        {
            try
            {
                // Include Items so we can list them in the delivery email
                var order = await _context.Orders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId)
                    ?? throw new OrderNotFoundException(orderId);

                order.DeliveryStatus = deliveryStatus;
                if (deliveryStatus == "Delivered")
                {
                    order.Status = "Delivered";
                    order.PaymentStatus = "Paid";

                    // Publish event so NotificationService sends delivery confirmation email
                    if (!string.IsNullOrEmpty(order.CustomerEmail))
                    {
                        var itemNames = order.Items.Any()
                            ? order.Items.Select(i => $"{i.MenuItemName} x{i.Quantity}").ToList()
                            : new List<string>();
                        _publisher.PublishOrderDelivered(
                            order.Id,
                            order.CustomerEmail,
                            order.CustomerName ?? "Customer",
                            itemNames
                        );
                    }
                }
                order.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (OrderNotFoundException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update delivery status: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetRestaurantEarningsAsync(Guid restaurantId)
        {
            try
            {
                var deliveredOrders = await _context.Orders
                    .Where(o => o.RestaurantId == restaurantId && o.Status == "Delivered")
                    .ToListAsync();

                return deliveredOrders.Sum(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to calculate restaurant earnings: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetAgentEarningsAsync(Guid agentId)
        {
            try
            {
                var deliveredOrders = await _context.Orders
                    .Where(o => o.DeliveryAgentId == agentId && o.DeliveryStatus == "Delivered")
                    .ToListAsync();

                // Agent earns: DeliveryFee × 3.5 per delivery
                return deliveredOrders.Sum(o => o.DeliveryFee * 3.5m);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to calculate agent earnings: {ex.Message}", ex);
            }
        }
    }
}
