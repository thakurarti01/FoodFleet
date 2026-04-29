using System.ComponentModel.DataAnnotations;

namespace RestaurantService.DTOs
{
    public class CreateRestaurantDto
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required] public string Address { get; set; } = string.Empty;
        public string CuisineTypes { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}
