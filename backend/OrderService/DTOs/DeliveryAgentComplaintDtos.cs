using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    public class CreateDeliveryAgentComplaintDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComplaintType { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }

    public class DeliveryAgentComplaintResponseDto
    {
        public int Id { get; set; }
        public Guid DeliveryAgentId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }
        public string ComplaintType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ResolveDeliveryAgentComplaintDto
    {
        [Required]
        [MaxLength(1000)]
        public string AdminResponse { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Resolved"; // Resolved or Dismissed
    }
}
