using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using ReviewService.Models;

namespace ReviewService.Data
{
	public class ReviewDbContext : DbContext
	{
		public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
		{
		}

		// Table for reviews
		public DbSet<Review> Reviews { get; set; }

		//  override OnModelCreating for extra configurations
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Unique constraint: 1 review per order per customer (FR-REV-06)
			modelBuilder.Entity<Review>()
				.HasIndex(r => new { r.OrderId, r.CustomerId })
				.IsUnique();

			//  Default values
			modelBuilder.Entity<Review>()
				.Property(r => r.IsDeleted)
				.HasDefaultValue(false);
		}
	}
}