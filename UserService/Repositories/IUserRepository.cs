using UserService.Models;

namespace UserService.Repositories
{
	public interface IUserRepository
	{
		Task<User> GetByEmailAsync(string email);
		Task AddAsync(User user);
	}
}
