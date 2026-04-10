namespace SearchDiscoveryService.DTOs
{
	public class SearchResultDTO
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string CuisineType { get; set; }

		public double Rating { get; set; }

		public double PriceAverage { get; set; }

		public int DeliveryTimeMinutes { get; set; }

		public double DistanceKm { get; set; }  // calculated

		public bool IsPromoted { get; set; }
	}
}