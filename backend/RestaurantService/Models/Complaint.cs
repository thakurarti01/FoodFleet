using System.ComponentModel.DataAnnotations;

namespace RestaurantService.Models
{
    /// <summary>
    /// Represents a complaint filed by a customer against a restaurant.
    /// After 5 complaints, the restaurant is automatically revoked for 1 month.
    /// </summary>
    public class Complaint
    {
        public int Id { get; set; }
        public Guid RestaurantId { get; set; }
        public Guid CustomerId { get; set; }
        public int OrderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComplaintType { get; set; } = string.Empty; // "Food Quality", "Service", "Hygiene", "Wrong Order", "Other"

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImageUrl { get; set; } // URL or path to uploaded image

        public string Status { get; set; } = "Pending"; // Pending, Resolved, Dismissed
        public string? AdminResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Restaurant? Restaurant { get; set; }
    }
}
