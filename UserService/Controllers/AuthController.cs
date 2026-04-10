using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers
{
	[ApiController]
	[Route("api/auth")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDTO dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState); // modelstate checks request validity
			}

			var result = await _authService.Register(dto);
			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDTO dto)
		{
			var result = await _authService.Login(dto);
			return Ok(result);
		}

		//[Authorize]
		//[HttpGet("profile")]
		//public IActionResult GetProfile()
		//{
		//	return Ok("You are authorized");
		//}
	}
}
