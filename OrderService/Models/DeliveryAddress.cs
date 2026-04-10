namespace OrderService.Models
{
	public class DeliveryAddress
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public string AddressLine { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string ZipCode { get; set; }
		public bool IsDefault { get; set; }
	}
}