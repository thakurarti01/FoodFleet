using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    /// <summary>
    /// DTO for placing a new order. Includes customer contact info
    /// so the notification service can send order confirmation emails.
    /// </summary>
    public class PlaceOrderDto
    {
        [Required] public Guid UserId { get; set; }
        [Required] public Guid RestaurantId { get; set; }
        [Required] public string DeliveryAddress { get; set; } = string.Empty;
        [Required] public List<OrderItemDto> Items { get; set; } = new();

        // Customer info passed through for email notifications
        public string? CustomerEmail { get; set; }
        public string? CustomerName { get; set; }
    }

    public class OrderItemDto
    {
        public int MenuItemId { get; set; }
        [Required] public string MenuItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? Customizations { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required] public string Status { get; set; } = string.Empty;
    }

    public class CancelOrderDto
    {
        [Required] public string Reason { get; set; } = string.Empty;
    }

    public class AssignAgentDto
    {
        [Required] public Guid DeliveryAgentId { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        [Required] public string DeliveryStatus { get; set; } = string.Empty;
    }
}
