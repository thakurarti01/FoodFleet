namespace UserService.Models
{
    // Entity class representing a user in the system
    // Used by Entity Framework Core to map to the 'Users' table in SQL Server
    public class User
    {
        // Guid (Globally Unique Identifier) used as primary key instead of int
        // Guid.NewGuid() auto-generates a unique ID when a new User object is created
        public Guid UserId { get; set; } = Guid.NewGuid();

        // { get; set; } is a C# auto-property — shorthand for getter and setter methods
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Never store plain-text passwords — always store the hashed version
        // BCrypt/PBKDF2 hash is stored here via ASP.NET Identity PasswordHasher
        public string PasswordHash { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        // Role-based access control — determines what endpoints the user can access
        // Default is "Customer"; other values: Admin, Owner, DeliveryAgent
        public string Role { get; set; } = "Customer";

        // Soft disable — instead of deleting the user, we mark them inactive
        public bool IsActive { get; set; } = true;

        // Can be used for email verification flow in future
        public bool IsVerified { get; set; } = false;

        // DateTime.UtcNow stores time in UTC (Coordinated Universal Time) — best practice for servers
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nullable string (?) — null means no reset is in progress
        // Token is generated and emailed to user when they request password reset
        public string? PasswordResetToken { get; set; }

        // Nullable DateTime — token expires after 30 minutes
        public DateTime? PasswordResetExpiry { get; set; }

        // Nullable — null means user is not suspended
        // Set to DateTime.UtcNow.AddMonths(1) when auto-suspended after 5 complaints
        public DateTime? SuspendedUntil { get; set; }

        // Human-readable reason stored alongside suspension date
        public string? SuspensionReason { get; set; }
    }
}
