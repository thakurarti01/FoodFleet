using SearchDiscoveryService.Models;

namespace SearchDiscoveryService.Data
{
	public static class SeedData
	{
		public static void Initialize(SearchDbContext context)
		{
			if (context.Restaurants.Any())
				return; // DB already seeded

			var restaurants = new List<Restaurant>
			{
				new Restaurant
				{
					Name = "Spice Hub",
					CuisineType = "Indian",
					Rating = 4.5,
					PriceAverage = 300,
					DeliveryTimeMinutes = 30,
					IsPromoted = true,
					Latitude = 28.6139,
					Longitude = 77.2090,
					MenuItems = new List<MenuItem>
					{
						new MenuItem { Name = "Butter Chicken", Description = "Creamy chicken curry", Price = 250 },
						new MenuItem { Name = "Paneer Tikka", Description = "Grilled paneer", Price = 200 }
					}
				},

				new Restaurant
				{
					Name = "Dragon Bowl",
					CuisineType = "Chinese",
					Rating = 4.2,
					PriceAverage = 250,
					DeliveryTimeMinutes = 25,
					IsPromoted = false,
					Latitude = 28.7041,
					Longitude = 77.1025,
					MenuItems = new List<MenuItem>
					{
						new MenuItem { Name = "Hakka Noodles", Description = "Stir-fried noodles", Price = 180 },
						new MenuItem { Name = "Manchurian", Description = "Veg balls in sauce", Price = 160 }
					}
				},

				new Restaurant
				{
					Name = "Pizza World",
					CuisineType = "Italian",
					Rating = 4.7,
					PriceAverage = 400,
					DeliveryTimeMinutes = 35,
					IsPromoted = true,
					Latitude = 28.5355,
					Longitude = 77.3910,
					MenuItems = new List<MenuItem>
					{
						new MenuItem { Name = "Margherita", Description = "Classic cheese pizza", Price = 299 },
						new MenuItem { Name = "Farmhouse Pizza", Description = "Veg loaded pizza", Price = 399 }
					}
				}
			};

			context.Restaurants.AddRange(restaurants);
			context.SaveChanges();
		}
	}
}