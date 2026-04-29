using UserService.Models;

namespace UserService.Repositories
{
	public interface IUserRepository
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<User> GetByEmailAsync(string email);
		Task<User?> GetByIdAsync(Guid userId);
		Task AddAsync(User user);
		Task<User?> GetByResetTokenAsync(string token);
		Task UpdateAsync(User user);
	}
}
