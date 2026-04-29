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
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();
        private readonly RabbitMQPublisher _publisher;

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
                if (dto.Password.Length < 6)
                    throw new WeakPasswordException();

                var existing = await _repo.GetByEmailAsync(dto.Email);
                if (existing != null)
                    throw new UserAlreadyExistsException(dto.Email);

                var allowedRoles = new[] { "Customer", "Admin", "Owner", "DeliveryAgent" };
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
                user.PasswordHash = _hasher.HashPassword(user, dto.Password);

                await _repo.AddAsync(user);

                try { _publisher.PublishUserRegistered(user.Email, user.FullName, user.Role); }
                catch { /* RabbitMQ optional */ }

                return "User registered successfully";
            }
            catch (WeakPasswordException) { throw; }
            catch (UserAlreadyExistsException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Registration failed: {ex.Message}", ex);
            }
        }

        public async Task<string> Login(LoginDTO dto)
        {
            try
            {
                var user = await _repo.GetByEmailAsync(dto.Email);
                if (user == null)
                    throw new InvalidCredentialsException();

                if (!user.IsActive)
                    throw new AccountDeactivatedException(dto.Email);

                var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
                if (result == PasswordVerificationResult.Failed)
                    throw new InvalidCredentialsException();

                return GenerateJwt(user);
            }
            catch (InvalidCredentialsException) { throw; }
            catch (AccountDeactivatedException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Login failed: {ex.Message}", ex);
            }
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("fullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> ForgotPassword(ForgotPasswordDTO dto)
        {
            try
            {
                var user = await _repo.GetByEmailAsync(dto.Email);
                if (user == null) return "ok"; // don't reveal if email exists

                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("+", "-").Replace("/", "_").Replace("=", "");

                user.PasswordResetToken = token;
                user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(30);
                await _repo.UpdateAsync(user);

                var frontendUrl = _config["Frontend:BaseUrl"];
                var resetLink = $"{frontendUrl}/auth/reset-password?token={token}";

                try
                {
                    var emailSettings = _config.GetSection("EmailSettings");
                    var email = new MimeMessage();
                    email.From.Add(MailboxAddress.Parse(emailSettings["From"]));
                    email.To.Add(MailboxAddress.Parse(user.Email));
                    email.Subject = "FoodFleet - Reset Your Password";
                    email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                    {
                        Text = $"Hi {user.FullName},\n\nReset your password here (valid 30 min):\n\n{resetLink}"
                    };
                    using var smtp = new SmtpClient();
                    await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["Port"]!), MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                catch { /* email failure is non-critical */ }

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
                var user = await _repo.GetByResetTokenAsync(dto.Token);
                if (user == null || user.PasswordResetExpiry < DateTime.UtcNow)
                    throw new InvalidResetTokenException();

                user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
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
