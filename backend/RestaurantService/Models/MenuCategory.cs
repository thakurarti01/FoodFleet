namespace RestaurantService.Models
{
    public class MenuCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}
