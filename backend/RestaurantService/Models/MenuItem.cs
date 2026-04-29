namespace RestaurantService.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string DietType { get; set; } = "Veg"; // Veg, Non-Veg, Vegan
        public int CategoryId { get; set; }

        public Restaurant? Restaurant { get; set; }
        public MenuCategory? Category { get; set; }
    }
}
