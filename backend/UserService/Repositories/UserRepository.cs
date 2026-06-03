using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories
{
    // Repository pattern — separates data access logic from business logic
    // Controllers and services call this instead of directly using DbContext
    // Makes code easier to test (can mock IUserRepository in unit tests)
    public class UserRepository : IUserRepository
    {
        // readonly — can only be assigned in constructor, prevents accidental reassignment
        // Underscore prefix (_context) is a naming convention for private fields
        private readonly AppDbContext _context;

        // Constructor injection — ASP.NET DI container provides AppDbContext automatically
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // async/await — non-blocking database call; frees the thread while waiting for DB response
        // Task<T> is the return type for async methods (like Promise in JavaScript)
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            // ToListAsync() executes the SQL SELECT query asynchronously
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            // FirstOrDefaultAsync returns the first match or null if not found
            // Lambda x => x.Email == email translates to SQL WHERE Email = @email
            return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            // ? on return type means this method can return null (nullable reference type)
            return await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(User user)
        {
            // AddAsync stages the new entity for insertion (does not hit DB yet)
            await _context.Users.AddAsync(user);
            // SaveChangesAsync commits all staged changes to the database in one transaction
            await _context.SaveChangesAsync();
        }

        // Used during password reset — finds user by their reset token
        public async Task<User?> GetByResetTokenAsync(string token)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token);
        }

        public async Task UpdateAsync(User user)
        {
            // Update marks the entity as modified — EF Core will generate an UPDATE SQL statement
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
