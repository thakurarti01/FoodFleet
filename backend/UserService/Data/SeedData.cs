using Microsoft.AspNetCore.Identity;
using UserService.Models;

namespace UserService.Data
{
    public static class SeedData
    {
        // Fixed GUIDs so RestaurantService seed can reference the same OwnerId
        public static readonly Guid ChandanId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        public static void Initialize(AppDbContext context)
        {
            var hasher = new PasswordHasher<User>();

            // Always ensure admin password is correct
            var existingAdmin = context.Users.FirstOrDefault(u => u.Email == "arti31thakur@gmail.com");
            if (existingAdmin != null)
            {
                existingAdmin.PasswordHash = hasher.HashPassword(existingAdmin, "Manager@123");
                existingAdmin.IsActive = true;
                existingAdmin.SuspendedUntil = null;
                existingAdmin.SuspensionReason = null;
                context.SaveChanges();
            }

            if (context.Users.Any()) return;

            var admin = new User
            {
                UserId = Guid.NewGuid(),
                FullName = "Arti Thakur",
                Email = "arti31thakur@gmail.com",
                PhoneNumber = "4708761961",
                Role = "Admin",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Manager@123");

            var chandan = new User
            {
                UserId = ChandanId,
                FullName = "Chandan Singh Rajput",
                Email = "devchandansr@gmail.com",
                PhoneNumber = "9876543210",
                Role = "Owner",
                IsActive = true,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            };
            chandan.PasswordHash = hasher.HashPassword(chandan, "Manager@123");

            context.Users.AddRange(admin, chandan);
            context.SaveChanges();
        }
    }
}
