using Microsoft.EntityFrameworkCore;
using RestaurantService.Models;

namespace RestaurantService.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
		public DbSet<Restaurant> Restaurants { get; set;}
	}
}
