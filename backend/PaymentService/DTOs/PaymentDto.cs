using System.ComponentModel.DataAnnotations;

namespace PaymentService.DTOs
{
    public class PayRequestDto
    {
        [Required] public int OrderId { get; set; }
        [Required] public Guid UserId { get; set; }
        [Required] public decimal Amount { get; set; }
        [Required] public string Method { get; set; } = "COD"; // COD or Card
    }

    public class RefundDto
    {
        [Required] public int PaymentId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
