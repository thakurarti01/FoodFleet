using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    public class VerifyOTPDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OTP { get; set; } = string.Empty;
    }
}
