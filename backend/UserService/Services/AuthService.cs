using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Models;
using UserService.Repositories;
using MailKit.Net.Smtp;
using MimeKit;

namespace UserService.Services
{
    /// <summary>
    /// Handles authentication: registration (with welcome email via RabbitMQ),
    /// JWT login, and password reset via email link.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;  // Reads values from appsettings.json (JWT keys, email settings etc.)

        // PasswordHasher<User> uses PBKDF2 algorithm to securely hash passwords
        // Never store plain-text passwords — always hash before saving to DB
        private readonly PasswordHasher<User> _hasher = new();

        private readonly RabbitMQPublisher _publisher; // Publishes events to RabbitMQ message queue

        // Constructor injection — all dependencies provided by ASP.NET DI container
        public AuthService(IUserRepository repo, IConfiguration config, RabbitMQPublisher publisher)
        {
            _repo = repo;
            _config = config;
            _publisher = publisher;
        }

        public async Task<string> Register(RegisterDTO dto)
        {
            try
            {
                // Validate minimum password length before doing any DB work
                if (dto.Password.Length < 6)
                    throw new WeakPasswordException();

                // Check for duplicate email — email must be unique per user
                var existing = await _repo.GetByEmailAsync(dto.Email);
                if (existing != null)
                    throw new UserAlreadyExistsException(dto.Email);

                // Whitelist of valid roles — prevents users from self-assigning Admin role
                var allowedRoles = new[] { "Customer", "Admin", "Owner", "DeliveryAgent" };
                // Ternary operator: if role is in allowed list use it, otherwise default to Customer
                var role = allowedRoles.Contains(dto.Role) ? dto.Role : "Customer";

                var user = new User
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber,
                    Role = role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Hash the password AFTER creating the user object (hasher uses user as salt context)
                user.PasswordHash = _hasher.HashPassword(user, dto.Password);

                await _repo.AddAsync(user);

                // Publish event to RabbitMQ — NotificationService will send welcome email
                // Wrapped in try/catch so registration still succeeds if RabbitMQ is down
                try { _publisher.PublishUserRegistered(user.Email, user.FullName, user.Role); }
                catch { /* RabbitMQ optional — email failure should not block registration */ }

                return "User registered successfully";
            }
            // Re-throw specific exceptions so the controller can return the correct HTTP status code
            catch (WeakPasswordException) { throw; }
            catch (UserAlreadyExistsException) { throw; }
            catch (Exception ex)
            {
                // Wrap unexpected exceptions with context for easier debugging
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }

        public async Task<string> Login(LoginDTO dto) //verifies credentials and returns JWT token if valid
        {
            try
            {
                var user = await _repo.GetByEmailAsync(dto.Email);
                // Return same error for wrong email or wrong password — prevents email enumeration attacks
                if (user == null)
                    throw new InvalidCredentialsException();

                if (!user.IsActive)
                {
                    // Check if the suspension period has expired — auto-reactivate if so
                    if (user.SuspendedUntil.HasValue && user.SuspendedUntil.Value <= DateTime.UtcNow)
                    {
                        // Suspension expired — restore account automatically
                        user.IsActive = true;
                        user.SuspendedUntil = null;
                        user.SuspensionReason = null;
                        await _repo.UpdateAsync(user);
                        // Fall through to password check below
                    }
                    else if (user.SuspendedUntil.HasValue)
                    {
                        // Still within suspension period — block login with details
                        throw new AccountSuspendedException(user.SuspendedUntil.Value, user.SuspensionReason ?? "Account suspended");
                    }
                    else
                    {
                        // Permanently deactivated by admin (no suspension date)
                        throw new AccountDeactivatedException(dto.Email);
                    }
                }

                // VerifyHashedPassword compares the stored hash against the provided plain-text password
                // Returns Failed, Success, or SuccessRehashNeeded
                var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
                if (result == PasswordVerificationResult.Failed)
                    throw new InvalidCredentialsException();

                // All checks passed — generate and return JWT token
                return GenerateJwt(user);
            }
            catch (InvalidCredentialsException) { throw; }
            catch (AccountDeactivatedException) { throw; }
            catch (AccountSuspendedException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        private string GenerateJwt(User user)
        {
            // SymmetricSecurityKey uses the same secret key for signing and verification
            // Key is read from appsettings.json Jwt:Key — must be at least 32 characters
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            // SigningCredentials specifies the key and algorithm used to sign the token
            // HmacSha256 is a widely used, secure signing algorithm for JWTs
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims are key-value pairs embedded inside the JWT token
            // They carry user identity info that the server can read without a DB call
            var claims = new[]
            {
                // NameIdentifier = user's unique ID (used to identify user in protected endpoints)
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                // Name = email address
                new Claim(ClaimTypes.Name, user.Email),
                // Custom claim for full name (not a standard ClaimType)
                new Claim("fullName", user.FullName),
                // Role claim — used by [Authorize(Roles = "Admin")] etc. for access control
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Build the JWT token with issuer, audience, claims, expiry, and signature
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],       // Who issued the token (FoodFleet)
                audience: _config["Jwt:Audience"],   // Who the token is for (FoodFleet)
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // Token valid for 8 hours
                signingCredentials: creds
            );

            // Serialize the token object to a compact string (the JWT string sent to client)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> ForgotPassword(ForgotPasswordDTO dto)
        {
            try
            {
                var user = await _repo.GetByEmailAsync(dto.Email);
                // Always return "ok" even if email doesn't exist — prevents email enumeration
                if (user == null) return "ok";

                // Generate a URL-safe random token using a GUID converted to Base64
                // Replace URL-unsafe characters (+, /, =) with safe alternatives
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "-").Replace("/", "_").Replace("=", "");

                // Store token and expiry in DB — validated when user clicks the reset link
                user.PasswordResetToken = token;
                user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(30); // Token valid for 30 minutes
                await _repo.UpdateAsync(user);

                var frontendUrl = _config["Frontend:BaseUrl"];
                var resetLink = $"{frontendUrl}/auth/reset-password?token={token}";

                try
                {
                    // Send reset email using MailKit SMTP client
                    var emailSettings = _config.GetSection("EmailSettings");
                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(emailSettings["From"]));
                    email.To.Add(MailboxAddress.Parse(user.Email));
                    email.Subject = "FoodFleet - Reset Your Password";
                    // TextFormat.Plain — plain text email (no HTML)
                    email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                    {
                        Text = $"Hi {user.FullName},\n\nReset your password here (valid 30 min):\n\n{resetLink}"
                    };
                    // 'using' ensures the SMTP client is disposed (connection closed) after use
                    using var smtp = new SmtpClient();
                    // StartTls = upgrade plain connection to encrypted TLS (port 587)
                    await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch { /* Email failure is non-critical — token is already saved in DB */ }

                return "ok";
            }
            catch (Exception ex)
            {
                throw new Exception($"Forgot password failed: {ex.Message}", ex);
            }
        }

        public async Task<string> ResetPassword(ResetPasswordDTO dto)
        {
            try
            {
                // Look up user by the reset token they received in email
                var user = await _repo.GetByResetTokenAsync(dto.Token);

                // Validate: token must exist AND not be expired
                if (user == null || user.PasswordResetExpiry < DateTime.UtcNow)
                    throw new InvalidResetTokenException();

                // Hash the new password before storing
                user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);

                // Clear the reset token so it cannot be reused
                user.PasswordResetToken = null;
                user.PasswordResetExpiry = null;
                await _repo.UpdateAsync(user);
                return "ok";
            }
            catch (InvalidResetTokenException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Password reset failed: {ex.Message}", ex);
            }
        }
    }
}
