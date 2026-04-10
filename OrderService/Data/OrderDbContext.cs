using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
	public class OrderDbContext : DbContext
	{
		public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
		{
		}

		public DbSet<Cart> Carts { get; set; }
		public DbSet<CartItem> CartItems { get; set; }

		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }

		public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }

		public DbSet<Notification> Notifications { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Cart - CartItem one-to-many
			modelBuilder.Entity<Cart>()
				.HasMany(c => c.Items)
				.WithOne(ci => ci.Cart)
				.HasForeignKey(ci => ci.CartId)
				.OnDelete(DeleteBehavior.Cascade);

			// Order - OrderItem one-to-many
			modelBuilder.Entity<Order>()
				.HasMany(o => o.Items)
				.WithOne(oi => oi.Order)
				.HasForeignKey(oi => oi.OrderId)
				.OnDelete(DeleteBehavior.Cascade);

			// Optional: Seed status values (Placed, Confirmed, etc.) if needed
		}
	}
}