using System.ComponentModel.DataAnnotations;

namespace ReviewService.DTOs
{
	public class UpdateReviewDTO
	{
		[Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
		public int? Rating { get; set; } // nullable, update only if provided

		[MaxLength(500)]
		public string? Comment { get; set; }
	}
}