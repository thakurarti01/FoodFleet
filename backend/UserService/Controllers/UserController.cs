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

        /// <summary>
        /// Retrieves all users in the system (Admin only)
        /// </summary>
        /// <returns>List of all users with summary information</returns>
        /// <response code="200">Returns list of users</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
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
        /// <returns>List of all delivery agents</returns>
        /// <response code="200">Returns list of delivery agents</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
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
        /// <param name="serviceKey">Internal service authentication key</param>
        /// <returns>List of all delivery agents for internal service use</returns>
        /// <response code="200">Returns list of delivery agents</response>
        /// <response code="401">Unauthorized - Invalid service key</response>
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

        /// <summary>
        /// Retrieves the authenticated user's profile information
        /// </summary>
        /// <returns>Current user's profile details</returns>
        /// <response code="200">Returns user profile</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="404">User not found</response>
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

        /// <summary>
        /// Updates a user's role (Admin only)
        /// </summary>
        /// <param name="userId">ID of the user to update</param>
        /// <param name="dto">New role assignment</param>
        /// <returns>Success message with updated role</returns>
        /// <response code="200">Role updated successfully</response>
        /// <response code="400">Invalid role specified</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        /// <response code="404">User not found</response>
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

        /// <summary>
        /// Deactivates a user account (Admin only)
        /// </summary>
        /// <param name="userId">ID of the user to deactivate</param>
        /// <returns>Success message</returns>
        /// <response code="200">User deactivated successfully</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        /// <response code="404">User not found</response>
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

        /// <summary>
        /// Suspends a user account with duration (Internal/Admin use)
        /// </summary>
        /// <param name="userId">ID of the user to suspend</param>
        /// <param name="dto">Suspension details including end date and reason</param>
        /// <returns>Success message</returns>
        /// <response code="200">User suspended successfully</response>
        /// <response code="404">User not found</response>
        [HttpPut("{userId}/suspend")]
        public async Task<IActionResult> Suspend(Guid userId, [FromBody] SuspendUserDTO dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            
            user.IsActive = false;
            user.SuspendedUntil = dto.SuspendedUntil;
            user.SuspensionReason = dto.SuspensionReason;
            
            await _repo.UpdateAsync(user);
            return Ok(new { 
                message = "User suspended", 
                userId = user.UserId, 
                suspendedUntil = user.SuspendedUntil,
                reason = user.SuspensionReason
            });
        }

        /// <summary>
        /// Reactivates a suspended user if suspension period has ended (Admin only)
        /// </summary>
        /// <param name="userId">ID of the user to reactivate</param>
        /// <returns>Success message</returns>
        /// <response code="200">User reactivated successfully</response>
        /// <response code="400">Suspension period not ended yet</response>
        /// <response code="401">Unauthorized - JWT token required</response>
        /// <response code="403">Forbidden - Admin role required</response>
        /// <response code="404">User not found</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return NotFound();
            
            if (user.SuspendedUntil.HasValue && user.SuspendedUntil.Value > DateTime.UtcNow)
            {
                return BadRequest(new { 
                    message = "Suspension period not ended yet", 
                    suspendedUntil = user.SuspendedUntil 
                });
            }
            
            user.IsActive = true;
            user.SuspendedUntil = null;
            user.SuspensionReason = null;
            
            await _repo.UpdateAsync(user);
            return Ok(new { message = "User reactivated", userId = user.UserId });
        }
    }

    /// <summary>
    /// Data transfer object for updating user role
    /// </summary>
    public class UpdateRoleDTO
    {
        /// <summary>
        /// New role to assign (Customer, Owner, DeliveryAgent, Admin)
        /// </summary>
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data transfer object for suspending a user
    /// </summary>
    public class SuspendUserDTO
    {
        /// <summary>
        /// Date and time when suspension ends
        /// </summary>
        public DateTime SuspendedUntil { get; set; }

        /// <summary>
        /// Reason for suspension
        /// </summary>
        public string SuspensionReason { get; set; } = string.Empty;
    }
}
