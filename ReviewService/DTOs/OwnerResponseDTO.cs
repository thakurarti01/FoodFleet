using System.ComponentModel.DataAnnotations;

namespace ReviewService.DTOs
{
	public class OwnerResponseDTO
	{
		[Required]
		public int ReviewId { get; set; }

		[Required]
		public int OwnerId { get; set; }

		[Required]
		[MaxLength(500)]
		public string ResponseText { get; set; } = string.Empty;
	}
}