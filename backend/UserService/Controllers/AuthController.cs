using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Services;

namespace UserService.Controllers
{
    /// <summary>
    /// Handles user registration, login, and password reset flows.
    /// Registration publishes a RabbitMQ event so NotificationService sends a welcome email.
    /// </summary>
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // AuthService.Register already publishes to user_registered_queue via RabbitMQPublisher
                var result = await _authService.Register(dto);
                return Ok(result);
            }
            catch (UserAlreadyExistsException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (WeakPasswordException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            try
            {
                var token = await _authService.Login(dto);
                return Ok(new { token });
            }
            catch (InvalidCredentialsException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (AccountDeactivatedException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _authService.ForgotPassword(dto);
                return Ok(new { message = "If that email exists, a reset link has been sent." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _authService.ResetPassword(dto);
                return Ok(new { message = "Password reset successfully." });
            }
            catch (InvalidResetTokenException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
