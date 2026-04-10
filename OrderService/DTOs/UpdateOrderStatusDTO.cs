namespace OrderService.DTOs
{
	public class UpdateOrderStatusDTO
	{
		public string Status { get; set; } // Placed, Confirmed, Preparing, Ready, PickedUp, Delivered
	}
}