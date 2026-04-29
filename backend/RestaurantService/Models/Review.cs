using System.ComponentModel.DataAnnotations;

namespace RestaurantService.Models
{
    public class Review
    {
        public int Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public string? OwnerResponse { get; set; }
        public DateTime? ResponseCreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Restaurant? Restaurant { get; set; }
    }
}
