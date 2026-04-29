using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Repositories;

namespace UserService.Controllers
{
    /// <summary>
    /// REST endpoints for user profile and admin user management.
    /// Customers can view their own profile; admins can list all users, update roles, and deactivate accounts.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo)
        {
            _repo = repo;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repo.GetAllAsync();
            var data = users.Select(u => new UserSummaryDTO
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
            return Ok(data);
        }

        /// <summary>
        /// Returns all delivery agents. Admin JWT required.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("delivery-agents")]
        public async Task<IActionResult> GetDeliveryAgents()
        {
            var users = await _repo.GetAllAsync();
            var agents = users
                .Where(u => u.Role == "DeliveryAgent")
                .Select(u => new UserSummaryDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });
            return Ok(agents);
        }

        /// <summary>
        /// Internal endpoint for OrderService auto-assignment. Requires service key header only.
        /// </summary>
        [HttpGet("delivery-agents/internal")]
        public async Task<IActionResult> GetDeliveryAgentsInternal([FromHeader(Name = "X-Service-Key")] string? serviceKey)
        {
            if (serviceKey != "foodfleet-internal-2024") return Unauthorized();

            var users = await _repo.GetAllAsync();
            var agents = users
                .Where(u => u.Role == "DeliveryAgent")
                .Select(u => new UserSummaryDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });
            return Ok(agents);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(new UserSummaryDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/role")]
        public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] UpdateRoleDTO dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var allowed = new[] { "Customer", "Admin", "Owner", "DeliveryAgent" };
            if (!allowed.Contains(dto.Role)) return BadRequest("Invalid role");

            user.Role = dto.Role;
            await _repo.UpdateAsync(user);
            return Ok(new { message = "Role updated", userId = user.UserId, role = user.Role });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            user.IsActive = false;
            await _repo.UpdateAsync(user);
            return Ok("User deactivated");
        }
    }

    public class UpdateRoleDTO
    {
        public string Role { get; set; } = string.Empty;
    }
}
