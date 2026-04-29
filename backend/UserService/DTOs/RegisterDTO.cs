using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
    public class RegisterDTO
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required, RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        // Allowed: Customer, Admin, Owner, DeliveryAgent
        public string Role { get; set; } = "Customer";
    }
}
