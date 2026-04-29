using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using UserService.DTOs;
using UserService.Models;
using UserService.Repositories;
using UserService.Services;

namespace UserService.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _repoMock = null!;
        private Mock<RabbitMQPublisher> _publisherMock = null!;
        private IConfiguration _config = null!;
        private AuthService _service = null!;

        // ── Setup ─────────────────────────────────────────────────────────────

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IUserRepository>();
            _publisherMock = new Mock<RabbitMQPublisher>();

            // In-memory config with JWT settings
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"]      = "THIS_IS_MY_SUPER_SECRET_KEY_1234567890",
                    ["Jwt:Issuer"]   = "FoodFleet",
                    ["Jwt:Audience"] = "FoodFleet",
                    ["Frontend:BaseUrl"] = "http://localhost:4200"
                })
                .Build();

            _service = new AuthService(_repoMock.Object, _config, _publisherMock.Object);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static RegisterDTO ValidRegisterDto(string email = "test@example.com") => new()
        {
            FullName    = "Test User",
            Email       = email,
            Password    = "password123",
            PhoneNumber = "9876543210",
            Role        = "Customer"
        };

        private static User HashedUser(string email = "test@example.com", string role = "Customer", bool isActive = true)
        {
            var user = new User
            {
                UserId      = Guid.NewGuid(),
                FullName    = "Test User",
                Email       = email,
                PhoneNumber = "9876543210",
                Role        = role,
                IsActive    = isActive
            };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, "password123");
            return user;
        }

        // ── Register ──────────────────────────────────────────────────────────

        [Test]
        public async Task Register_NewUser_ReturnsSuccessMessage()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _service.Register(ValidRegisterDto());

            Assert.That(result, Is.EqualTo("User registered successfully"));
        }

        [Test]
        public async Task Register_DuplicateEmail_ReturnsUserAlreadyExists()
        {
            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com"))
                     .ReturnsAsync(HashedUser("test@example.com"));

            var result = await _service.Register(ValidRegisterDto("test@example.com"));

            Assert.That(result, Is.EqualTo("User already exists"));
        }

        [Test]
        public void Register_WeakPassword_ThrowsException()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var dto = ValidRegisterDto();
            dto.Password = "123"; // less than 6 chars

            Assert.ThrowsAsync<Exception>(() => _service.Register(dto));
        }

        [Test]
        public async Task Register_InvalidRole_DefaultsToCustomer()
        {
            User? savedUser = null;
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .Callback<User>(u => savedUser = u)
                     .Returns(Task.CompletedTask);

            var dto = ValidRegisterDto();
            dto.Role = "SuperHacker"; // invalid role

            await _service.Register(dto);

            Assert.That(savedUser!.Role, Is.EqualTo("Customer"));
        }

        [Test]
        public async Task Register_ValidOwnerRole_SavesOwnerRole()
        {
            User? savedUser = null;
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .Callback<User>(u => savedUser = u)
                     .Returns(Task.CompletedTask);

            var dto = ValidRegisterDto();
            dto.Role = "Owner";

            await _service.Register(dto);

            Assert.That(savedUser!.Role, Is.EqualTo("Owner"));
        }

        [Test]
        public async Task Register_NewUser_CallsAddAsync()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            await _service.Register(ValidRegisterDto());

            _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task Register_NewUser_PasswordIsHashed()
        {
            User? savedUser = null;
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                     .Callback<User>(u => savedUser = u)
                     .Returns(Task.CompletedTask);

            await _service.Register(ValidRegisterDto());

            // Password should be hashed, not plain text
            Assert.That(savedUser!.PasswordHash, Is.Not.EqualTo("password123"));
            Assert.That(savedUser.PasswordHash, Does.StartWith("AQ")); // BCrypt/PBKDF2 prefix
        }

        // ── Login ─────────────────────────────────────────────────────────────

        [Test]
        public async Task Login_ValidCredentials_ReturnsJwtToken()
        {
            var user = HashedUser("test@example.com");
            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var result = await _service.Login(new LoginDTO { Email = "test@example.com", Password = "password123" });

            // JWT tokens have 3 dot-separated parts
            Assert.That(result.Split('.').Length, Is.EqualTo(3));
        }

        [Test]
        public async Task Login_WrongPassword_ReturnsInvalidCredentials()
        {
            var user = HashedUser("test@example.com");
            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var result = await _service.Login(new LoginDTO { Email = "test@example.com", Password = "wrongpassword" });

            Assert.That(result, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task Login_NonExistentEmail_ReturnsInvalidCredentials()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await _service.Login(new LoginDTO { Email = "nobody@example.com", Password = "password123" });

            Assert.That(result, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task Login_DeactivatedUser_ReturnsAccountDeactivated()
        {
            var user = HashedUser("test@example.com", isActive: false);
            _repoMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

            var result = await _service.Login(new LoginDTO { Email = "test@example.com", Password = "password123" });

            Assert.That(result, Is.EqualTo("Account is deactivated"));
        }

        // ── ResetPassword ─────────────────────────────────────────────────────

        [Test]
        public async Task ResetPassword_ValidToken_ReturnsOkAndClearsToken()
        {
            var user = HashedUser();
            user.PasswordResetToken = "valid-token";
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(10);

            _repoMock.Setup(r => r.GetByResetTokenAsync("valid-token")).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            var result = await _service.ResetPassword(new ResetPasswordDTO
            {
                Token = "valid-token",
                NewPassword = "newpassword123"
            });

            Assert.That(result, Is.EqualTo("ok"));
            Assert.That(user.PasswordResetToken, Is.Null);
            Assert.That(user.PasswordResetExpiry, Is.Null);
        }

        [Test]
        public async Task ResetPassword_ExpiredToken_ReturnsInvalid()
        {
            var user = HashedUser();
            user.PasswordResetToken = "expired-token";
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(-5); // expired

            _repoMock.Setup(r => r.GetByResetTokenAsync("expired-token")).ReturnsAsync(user);

            var result = await _service.ResetPassword(new ResetPasswordDTO
            {
                Token = "expired-token",
                NewPassword = "newpassword123"
            });

            Assert.That(result, Is.EqualTo("invalid"));
        }

        [Test]
        public async Task ResetPassword_InvalidToken_ReturnsInvalid()
        {
            _repoMock.Setup(r => r.GetByResetTokenAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            var result = await _service.ResetPassword(new ResetPasswordDTO
            {
                Token = "nonexistent-token",
                NewPassword = "newpassword123"
            });

            Assert.That(result, Is.EqualTo("invalid"));
        }

        [Test]
        public async Task ResetPassword_ValidToken_PasswordIsRehashed()
        {
            var user = HashedUser();
            var oldHash = user.PasswordHash;
            user.PasswordResetToken = "valid-token";
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(10);

            _repoMock.Setup(r => r.GetByResetTokenAsync("valid-token")).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            await _service.ResetPassword(new ResetPasswordDTO
            {
                Token = "valid-token",
                NewPassword = "brandnewpassword"
            });

            Assert.That(user.PasswordHash, Is.Not.EqualTo(oldHash));
        }

        // ── ForgotPassword ────────────────────────────────────────────────────

        [Test]
        public async Task ForgotPassword_NonExistentEmail_ReturnsOkWithoutError()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            // Should not throw — avoids email enumeration
            var result = await _service.ForgotPassword(new ForgotPasswordDTO { Email = "nobody@example.com" });

            Assert.That(result, Is.EqualTo("ok"));
        }

        [Test]
        public async Task ForgotPassword_ExistingEmail_SetsResetTokenAndExpiry()
        {
            var user = HashedUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            await _service.ForgotPassword(new ForgotPasswordDTO { Email = user.Email });

            Assert.That(user.PasswordResetToken, Is.Not.Null);
            Assert.That(user.PasswordResetExpiry, Is.GreaterThan(DateTime.UtcNow));
        }
    }
}
