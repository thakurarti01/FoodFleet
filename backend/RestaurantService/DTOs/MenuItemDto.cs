using System.ComponentModel.DataAnnotations;

namespace RestaurantService.DTOs
{
    public class CreateMenuItemDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string DietType { get; set; } = "Veg";
        public int CategoryId { get; set; }
    }

    public class UpdateMenuItemDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsAvailable { get; set; }
        public string? DietType { get; set; }
    }
}
