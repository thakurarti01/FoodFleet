using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    /// <summary>
    /// Represents a rating and review for a delivery agent by a customer.
    /// One rating per customer per order.
    /// </summary>
    public class DeliveryAgentRating
    {
        public int Id { get; set; }
        public Guid DeliveryAgentId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
