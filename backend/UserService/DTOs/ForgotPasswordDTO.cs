using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
	public class ForgotPasswordDTO
	{
		[Required, EmailAddress]
		public string Email { get; set; }
	}
}
