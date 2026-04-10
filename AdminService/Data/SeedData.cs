using AdminService.Models;

namespace AdminService.Data
{
	public static class SeedData
	{
		public static void Initialize(AdminDbContext context)
		{
			if (context.Users.Any() || context.Restaurants.Any() || context.DeliveryAgents.Any())
				return;

			// Users
			context.Users.AddRange(
				new User { Name = "Arti", Email = "arti@gmail.com", IsActive = true },
				new User { Name = "Rahul", Email = "rahul@gmail.com", IsActive = true }
			);

			// Restaurants
			context.Restaurants.AddRange(
				new Restaurant { Name = "Food Hub", OwnerName = "Rahul", IsApproved = false, IsRejected = false },
				new Restaurant { Name = "Spice Villa", OwnerName = "Amit", IsApproved = true, IsRejected = false }
			);

			// Delivery Agents
			context.DeliveryAgents.AddRange(
				new DeliveryAgent { Name = "Aman", VehicleType = "Bike", IsAvailable = true },
				new DeliveryAgent { Name = "Rohit", VehicleType = "Scooter", IsAvailable = true }
			);

			context.SaveChanges();
		}
	}
}