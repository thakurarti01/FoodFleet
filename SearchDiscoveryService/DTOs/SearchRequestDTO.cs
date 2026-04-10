namespace SearchDiscoveryService.DTOs
{
	public class SearchRequestDTO
	{
		public string? Query { get; set; }

		public string? CuisineType { get; set; }

		public double? MinRating { get; set; }

		public double? MaxPrice { get; set; }

		public int? MaxDeliveryTime { get; set; }

		// User location (for proximity sorting)
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}