using UserService.DTOs;

namespace UserService.Services
{
	public interface IAuthService
	{
		Task<string> Register(RegisterDTO dto);
		Task<string> Login(LoginDTO dto);
	}
}
