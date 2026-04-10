namespace SearchDiscoveryService.Models
{
	public class Restaurant
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string CuisineType { get; set; }   // e.g., Indian, Chinese

		public double Rating { get; set; }

		public double PriceAverage { get; set; }

		public int DeliveryTimeMinutes { get; set; }

		public bool IsPromoted { get; set; } = false;   // FR-SRCH-04

		// Location (for proximity search)
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		// Navigation
		public List<MenuItem> MenuItems { get; set; }
	}
}