using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User> GetByEmailAsync(string email)
		{
			return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
		}

		public async Task<User?> GetByIdAsync(Guid userId)
		{
			return await _context.Users.FirstOrDefaultAsync(x => x.UserId == userId);
		}

		public async Task AddAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}

		public async Task<User?> GetByResetTokenAsync(string token)
		{
			return await _context.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == token);
		}

		public async Task UpdateAsync(User user)
		{
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
		}
	}
}
