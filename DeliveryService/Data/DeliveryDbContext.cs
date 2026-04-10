using Microsoft.EntityFrameworkCore;
using DeliveryService.Models;

namespace DeliveryService.Data
{
	public class DeliveryDbContext : DbContext
	{
		public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
			: base(options)
		{
		}

		public DbSet<DeliveryAgent> DeliveryAgents { get; set; }
		public DbSet<Delivery> Deliveries { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Relationship: One Agent → Many Deliveries
			modelBuilder.Entity<Delivery>()
				.HasOne(d => d.Agent)
				.WithMany(a => a.Deliveries)
				.HasForeignKey(d => d.AgentId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}