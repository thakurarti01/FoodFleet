using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
	public class NotificationDbContext : DbContext
	{
		public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
			: base(options)
		{
		}

		// DbSets
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<UserNotificationSetting> UserNotificationSettings { get; set; }

		// Add Users DbSet for dynamic email
		public DbSet<User> Users { get; set; }

		// Optional: configure relationships or table names
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Notification>(entity =>
			{
				entity.HasKey(n => n.Id);
				entity.Property(n => n.RecipientId).IsRequired();
				entity.Property(n => n.Message).IsRequired();
				entity.Property(n => n.Type).IsRequired();
			});

			modelBuilder.Entity<UserNotificationSetting>(entity =>
			{
				entity.HasKey(s => s.Id);
				entity.Property(s => s.UserId).IsRequired();
			});

			// Configure User entity
			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(u => u.Id);
				entity.Property(u => u.Name).IsRequired();
				entity.Property(u => u.Email).IsRequired();
			});
		}
	}
}