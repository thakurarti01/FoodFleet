using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

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
				return BadRequest(ModelState);
			}

			// STEP 1: register user (your existing logic)
			var result = await _authService.Register(dto);

			// STEP 2: send message to RabbitMQ for email notification
			var factory = new ConnectionFactory()
			{
				HostName = "localhost"
			};

			using var connection = factory.CreateConnection();
			using var channel = connection.CreateModel();

			channel.QueueDeclare(
				queue: "user_registered_queue",
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			// message to NotificationService
			var message = new
			{
				Email = dto.Email,
				FullName = dto.FullName
			};

			var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

			channel.BasicPublish(
				exchange: "",
				routingKey: "user_registered_queue",
				body: body
			);

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
