using System.ComponentModel.DataAnnotations;

namespace RestaurantService.DTOs
{
    public class CreateReviewDto
    {
        [Required] public Guid CustomerId { get; set; }
        [Required] public int OrderId { get; set; }
        [Range(1, 5)] public int Rating { get; set; }
        [MaxLength(500)] public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        [Range(1, 5)] public int? Rating { get; set; }
        [MaxLength(500)] public string? Comment { get; set; }
    }

    public class OwnerResponseDto
    {
        [Required] public string ResponseText { get; set; } = string.Empty;
    }
}
