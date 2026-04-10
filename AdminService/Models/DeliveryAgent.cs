namespace AdminService.Models
{
	public class DeliveryAgent
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string VehicleType { get; set; }
		public bool IsAvailable { get; set; } = true;
	}
}