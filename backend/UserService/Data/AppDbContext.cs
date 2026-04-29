using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) { }

		// email must be unique in database
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<User>() // targeting User table
				.HasIndex(u => u.Email) // creating index on email for faster search 
				.IsUnique();
		}
		public DbSet<User> Users { get; set; }
	}
}
