using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryAgentRating> DeliveryAgentRatings { get; set; }
        public DbSet<DeliveryAgentComplaint> DeliveryAgentComplaints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .HasMany<DeliveryAgentRating>()
                .WithOne(r => r.Order)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasMany<DeliveryAgentComplaint>()
                .WithOne(c => c.Order)
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DeliveryAgentRating>()
                .HasIndex(r => new { r.OrderId, r.CustomerId })
                .IsUnique();

            modelBuilder.Entity<DeliveryAgentComplaint>()
                .HasIndex(c => new { c.OrderId, c.CustomerId });
        }
    }
}
