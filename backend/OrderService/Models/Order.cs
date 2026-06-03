namespace OrderService.Models
{
    // Entity class representing a food delivery order
    // Maps to the 'Orders' table in SQL Server via Entity Framework Core
    public class Order
    {
        // int primary key — auto-incremented by SQL Server (IDENTITY column)
        public int Id { get; set; }

        // Foreign key reference to the customer who placed the order (from UserService)
        // Guid is used instead of int to avoid exposing sequential IDs
        public Guid UserId { get; set; }

        // Foreign key to the restaurant (from RestaurantService)
        public Guid RestaurantId { get; set; }

        // Nullable Guid — null means no agent has been assigned yet
        // Assigned automatically when order is placed (auto-assignment algorithm)
        public Guid? DeliveryAgentId { get; set; }

        // Calculated as sum of (Price × Quantity) for all order items
        public decimal TotalAmount { get; set; }

        // Fixed delivery fee charged per order — default ₹40
        // Agent earns DeliveryFee × 3.5 per completed delivery
        public decimal DeliveryFee { get; set; } = 40;

        // Order lifecycle status — tracks restaurant-side progress
        // Flow: Placed → Confirmed → Preparing → Ready → Delivered / Cancelled
        public string Status { get; set; } = "Placed";

        // Delivery-side status — tracks agent progress independently
        // Flow: Pending → Assigned → PickedUp → Delivered / Cancelled
        public string DeliveryStatus { get; set; } = "Pending";

        // Payment status — COD starts as Pending, set to Paid on delivery
        public string PaymentStatus { get; set; } = "Pending";

        public string DeliveryAddress { get; set; } = string.Empty;

        // Nullable — only set when order is cancelled
        public string? CancellationReason { get; set; }

        // Denormalized fields — stored at order time to avoid cross-service HTTP calls
        // when sending notification emails (NotificationService needs these)
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public string? RestaurantName { get; set; }

        // 6-digit OTP generated when delivery agent is assigned
        // Customer shares this with agent at doorstep to confirm delivery
        public string? DeliveryOTP { get; set; }

        // Timestamp of OTP generation — can be used to implement OTP expiry
        public DateTime? OTPGeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nullable — only set when order is updated after creation
        public DateTime? UpdatedAt { get; set; }

        // Navigation property — EF Core loads related OrderItems when .Include(o => o.Items) is used
        // Initialized to empty list to avoid null reference exceptions
        public List<OrderItem> Items { get; set; } = new();
    }
}
