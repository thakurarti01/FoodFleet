namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid? DeliveryAgentId { get; set; }  // nullable, assigned later
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; } = 40; // Standard delivery fee
        public string Status { get; set; } = "Placed"; // Placed, Confirmed, Preparing, Ready, Delivered, Cancelled
        public string DeliveryStatus { get; set; } = "Pending"; // Pending, Assigned, PickedUp, Delivered
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? CancellationReason { get; set; }
        // Stored at order time for notification emails (avoids cross-service calls)
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
        public string? RestaurantName { get; set; }
        public string? DeliveryOTP { get; set; }
        public DateTime? OTPGeneratedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
