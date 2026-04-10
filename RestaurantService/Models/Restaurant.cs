
namespace RestaurantService.Models
{
	public class Restaurant
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid OwnerId { get; set; }

		public string Name { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public string Address { get; set; } = string.Empty;

		public string CuisineTypes { get; set; } = string.Empty;

		public string? LogoUrl { get; set; }

		public double AverageRating { get; set; } = 0;

		public int TotalReviews { get; set; } = 0;

		public string Status { get; set; } = "Pending";

		public bool IsOpen { get; set; } = false;

		public string OperatingHours { get; set; } = string.Empty;

		public double MinimumOrderAmount { get; set; }

		public int EstimatedDeliveryMinutes { get; set; }

	}
}
