using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
	public class RegisterDTO
	{
		[Required(ErrorMessage = "Full name is required")]
		public string FullName { get; set; }

		[Required]
		[EmailAddress(ErrorMessage = "Invalid email")]
		public string Email { get; set; }

		[Required]
		[MinLength(6, ErrorMessage = "Password must be at least 6 chars")]
		public string Password { get; set; }

		[Required]
		[RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid mobile number")]
		public string MobileNumber { get; set; }

		// Allowed: Customer, Admin, RestaurantOwner, DeliveryAgent
		public string Role { get; set; } = "Customer";
	}
}
