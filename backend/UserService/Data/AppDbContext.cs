using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    // DbContext is the EF Core class that acts as a bridge between C# models and the database
    // It manages database connections, tracks changes, and executes queries
    public class AppDbContext : DbContext
    {
        // Constructor receives DbContextOptions (connection string, provider etc.) via Dependency Injection
        // 'base(options)' passes the options up to the parent DbContext class
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet<User> represents the 'Users' table — allows LINQ queries like _context.Users.Where(...)
        public DbSet<User> Users { get; set; }

        // OnModelCreating is called by EF Core when building the database schema
        // Used to configure relationships, indexes, and constraints beyond what attributes can do
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Entity<User>() targets the User table for configuration
            modelBuilder.Entity<User>()
                // HasIndex creates a database index on the Email column for faster lookups
                .HasIndex(u => u.Email)
                // IsUnique enforces that no two users can have the same email address
                // This is a database-level constraint — EF Core will throw on duplicate email insert
                .IsUnique();
        }
    }
}
