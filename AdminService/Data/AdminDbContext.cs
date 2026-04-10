using Microsoft.EntityFrameworkCore;
using AdminService.Models;

namespace AdminService.Data
{
	public class AdminDbContext : DbContext
	{
		public AdminDbContext(DbContextOptions<AdminDbContext> options)
			: base(options)
		{
		}

		// Tables
		public DbSet<Admin> Admins { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<Restaurant> Restaurants { get; set; }
		public DbSet<DeliveryAgent> DeliveryAgents { get; set; }
		public DbSet<Dispute> Disputes { get; set; }
		public DbSet<Refund> Refunds { get; set; }
		public DbSet<Banner> Banners { get; set; }
		public DbSet<FeaturedRestaurant> FeaturedRestaurants { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Example Configurations

			modelBuilder.Entity<Admin>()
				.Property(a => a.Name)
				.IsRequired()
				.HasMaxLength(100);

			modelBuilder.Entity<Admin>()
				.Property(a => a.Email)
				.IsRequired();

			modelBuilder.Entity<Restaurant>()
				.Property(r => r.Name)
				.IsRequired();

			modelBuilder.Entity<Dispute>()
				.Property(d => d.Status)
				.HasDefaultValue("Pending");

			modelBuilder.Entity<Refund>()
				.Property(r => r.Status)
				.HasDefaultValue("Initiated");

			modelBuilder.Entity<Banner>()
				.Property(b => b.IsActive)
				.HasDefaultValue(true);
		}
	}
}