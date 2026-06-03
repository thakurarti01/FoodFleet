using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    // EF Core DbContext for the OrderService database
    // Manages Orders, OrderItems, DeliveryAgentRatings, and DeliveryAgentComplaints tables
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        // DbSet properties represent database tables
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<DeliveryAgentRating> DeliveryAgentRatings { get; set; }
        public DbSet<DeliveryAgentComplaint> DeliveryAgentComplaints { get; set; }

        // Fluent API configuration — defines relationships and constraints in code
        // Runs once when EF Core builds the model (not on every request)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One Order has many OrderItems; if Order is deleted, all its items are deleted too (Cascade)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Explicit decimal precision — prevents SQL Server from silently truncating values
            // decimal(18,2) = up to 18 digits total, 2 after decimal point (e.g., 99999999999999.99)
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.DeliveryFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            // One Order can have many ratings (though business logic limits to one per customer per order)
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

            // Composite unique index — prevents a customer from rating the same order twice
            // Database-level constraint as a safety net alongside service-level checks
            modelBuilder.Entity<DeliveryAgentRating>()
                .HasIndex(r => new { r.OrderId, r.CustomerId })
                .IsUnique();

            // Non-unique index on complaints — allows multiple complaints per order
            // but improves query performance when filtering by OrderId + CustomerId
            modelBuilder.Entity<DeliveryAgentComplaint>()
                .HasIndex(c => new { c.OrderId, c.CustomerId });
        }
    }
}
