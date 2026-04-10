namespace RestaurantService.DTO
{
	public class CreateRestaurantDto
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;

		public string CuisineTypes { get; set; } = string.Empty;
		public string? LogoUrl { get; set; }

		public string OperatingHours { get; set; } = string.Empty;
		public double MinimumOrderAmount { get; set; }
		public int EstimatedDeliveryMinutes { get; set; }

	}
}
