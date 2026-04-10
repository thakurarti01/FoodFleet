using System.ComponentModel.DataAnnotations;

namespace ReviewService.DTOs
{
	public class CreateReviewDTO
	{
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
	}
}