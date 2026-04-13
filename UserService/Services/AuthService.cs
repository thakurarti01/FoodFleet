using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _repo;
		private readonly IConfiguration _config;
		private readonly PasswordHasher<User> _hasher = new();

		//  RabbitMQ Publisher
		private readonly RabbitMQPublisher _publisher;

		public AuthService(IUserRepository repo, IConfiguration config, RabbitMQPublisher publisher)
		{
			_repo = repo;
			_config = config;
			_publisher = publisher;
		}

		public async Task<string> Register(RegisterDTO dto)
		{
			if (dto.Password.Length < 6)
			{
				throw new Exception("Weak password");
			}

			var existing = await _repo.GetByEmailAsync(dto.Email);
			if (existing != null)
			{
				return "User already exists";
			}

			var allowedRoles = new[] { "Customer", "Admin", "RestaurantOwner", "DeliveryAgent" };
			var role = allowedRoles.Contains(dto.Role) ? dto.Role : "Customer";

			var user = new User
			{
				UserId = Guid.NewGuid(),
				FullName = dto.FullName,
				Email = dto.Email,
				MobileNumber = dto.MobileNumber,
				Role = role,
				CreatedAt = DateTime.Now,
				IsVerified = false
			};

			user.PasswordHash = _hasher.HashPassword(user, dto.Password);

			//  Save user
			await _repo.AddAsync(user);

			//  Publish event to RabbitMQ
			_publisher.PublishUserRegistered(user.Email, user.FullName);

			return "User registered successfully";
		}

		public async Task<string> Login(LoginDTO dto)
		{
			var user = await _repo.GetByEmailAsync(dto.Email);

			if (user == null)
			{
				return "Invalid credentials";
			}

			var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

			if (result == PasswordVerificationResult.Failed)
			{
				return "Invalid credentials";
			}

			return GenerateJwt(user);
		}

		private string GenerateJwt(User user)
		{
			var key = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(_config["Jwt:Key"])
			);

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new Claim(ClaimTypes.Name, user.Email),
				new Claim(ClaimTypes.Role, user.Role)
			};

			var token = new JwtSecurityToken(
				issuer: "FoodFleet",
				audience: "FoodFleet",
				claims: claims,
				expires: DateTime.UtcNow.AddHours(2),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}