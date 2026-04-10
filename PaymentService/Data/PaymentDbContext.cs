using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data
{
	public class PaymentDbContext : DbContext
	{
		public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
			: base(options)
		{
		}

		// Tables
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Refund> Refunds { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Payment Configuration
			modelBuilder.Entity<Payment>(entity =>
			{
				entity.HasKey(p => p.Id);

				entity.Property(p => p.Amount)
					  .IsRequired()
					  .HasColumnType("decimal(18,2)");

				entity.Property(p => p.PaymentMethod)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(p => p.Status)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(p => p.TransactionId)
					  .HasMaxLength(100);
			});

			// Refund Configuration
			modelBuilder.Entity<Refund>(entity =>
			{
				entity.HasKey(r => r.Id);

				entity.Property(r => r.Amount)
					  .IsRequired()
					  .HasColumnType("decimal(18,2)");

				entity.Property(r => r.Status)
					  .IsRequired()
					  .HasMaxLength(50);

				entity.Property(r => r.Reason)
					  .HasMaxLength(250);
			});
		}
	}
}