using MenuService.Models;
using Microsoft.EntityFrameworkCore;
namespace MenuService.Data
{
	public class MenuDbContext : DbContext
	{
		public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }

		public DbSet<MenuCategory> Categories { get; set; }
		public DbSet<MenuItem> Items { get; set; }
		public DbSet<MenuItemOption> Options { get; set; }
	}
}
