using System.Text.Json.Serialization;
namespace MenuService.Models
{
	public class MenuItemOption
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal AdditionalPrice { get; set; }

		public int MenuItemId { get; set; }
		[JsonIgnore]
		public MenuItem? MenuItem { get; set; }
	}
}
