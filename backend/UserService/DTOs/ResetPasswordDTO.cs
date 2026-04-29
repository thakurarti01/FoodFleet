using System.ComponentModel.DataAnnotations;

namespace UserService.DTOs
{
	public class ResetPasswordDTO
	{
		[Required]
		public string Token { get; set; }

		[Required, MinLength(6)]
		public string NewPassword { get; set; }
	}
}
