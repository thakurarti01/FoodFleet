using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    /// <summary>
    /// Represents a complaint filed by a customer against a delivery agent.
    /// After 5 complaints, the agent is automatically suspended for 1 month.
    /// </summary>
    public class DeliveryAgentComplaint
    {
        public int Id { get; set; }
        public Guid DeliveryAgentId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComplaintType { get; set; } = string.Empty; // "Late Delivery", "Rude Behavior", "Wrong Address", "Damaged Food", "Other"

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Resolved, Dismissed
        public string? AdminResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Order? Order { get; set; }
    }
}
