namespace UserService.Models
{
	public class User
	{
		public Guid UserId { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public string MobileNumber { get; set; }
		public string Role { get; set; }
		public bool IsVerified { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
