using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using SearchDiscoveryService.Models;

namespace SearchDiscoveryService.Data
{
	public class SearchDbContext : DbContext
	{
		public SearchDbContext(DbContextOptions<SearchDbContext> options)
			: base(options)
		{
		}

		public DbSet<Restaurant> Restaurants { get; set; }
		public DbSet<MenuItem> MenuItems { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Relationship: One Restaurant -> Many MenuItems
			modelBuilder.Entity<MenuItem>()
				.HasOne(m => m.Restaurant)
				.WithMany(r => r.MenuItems)
				.HasForeignKey(m => m.RestaurantId)
				.OnDelete(DeleteBehavior.Cascade);

			// Optional: Add indexes for faster search
			modelBuilder.Entity<Restaurant>()
				.HasIndex(r => r.Name);

			modelBuilder.Entity<MenuItem>()
				.HasIndex(m => m.Name);
		}
	}
}