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

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="dto">Registration details including email, password, full name, phone number, and role</param>
        /// <returns>JWT token and user details on success</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Invalid input or weak password</response>
        /// <response code="409">User with email already exists</response>
        /// <response code="500">Internal server error</response>
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

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="dto">Login credentials (email and password)</param>
        /// <returns>JWT token for authenticated requests</returns>
        /// <response code="200">Login successful, returns JWT token</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="403">Account is deactivated or suspended</response>
        /// <response code="500">Internal server error</response>
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
                return StatusCode(403, new { message = ex.Message });
            }
            catch (AccountSuspendedException ex)
            {
                return StatusCode(403, new { 
                    message = ex.Message,
                    suspendedUntil = ex.SuspendedUntil,
                    reason = ex.Reason
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Initiates password reset process by sending reset token to user's email
        /// </summary>
        /// <param name="dto">Email address for password reset</param>
        /// <returns>Success message (always returns success for security)</returns>
        /// <response code="200">Reset email sent if account exists</response>
        /// <response code="400">Invalid input</response>
        /// <response code="500">Internal server error</response>
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

        /// <summary>
        /// Resets user password using the token sent via email
        /// </summary>
        /// <param name="dto">Reset token and new password</param>
        /// <returns>Success message on password reset</returns>
        /// <response code="200">Password reset successfully</response>
        /// <response code="400">Invalid or expired reset token</response>
        /// <response code="500">Internal server error</response>
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
