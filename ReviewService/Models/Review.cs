using System.ComponentModel.DataAnnotations;

namespace ReviewService.Models
{
	public class Review
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int OrderId { get; set; }

		[Required]
		public int CustomerId { get; set; }

		[Required]
		public int RestaurantId { get; set; }

		[Required]
		[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
		public int Rating { get; set; }

		[MaxLength(500)]
		public string? Comment { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Owner response (FR-REV-04)
		public string? OwnerResponse { get; set; }

		public DateTime? ResponseCreatedAt { get; set; }

		// Admin delete (FR-REV-05)
		public bool IsDeleted { get; set; } = false;
	}
}