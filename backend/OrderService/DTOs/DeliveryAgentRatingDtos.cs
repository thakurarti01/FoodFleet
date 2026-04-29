using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    public class CreateDeliveryAgentRatingDto
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }
    }

    public class DeliveryAgentRatingResponseDto
    {
        public int Id { get; set; }
        public Guid DeliveryAgentId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
