namespace OrderService.Models
{
    // Represents a single line item within an order
    // Stored separately so one order can have multiple items (one-to-many relationship)
    public class OrderItem
    {
        public int Id { get; set; }

        // Foreign key linking this item back to its parent Order
        public int OrderId { get; set; }

        // Reference to the menu item in RestaurantService
        // We don't use a foreign key here because it's a cross-service reference
        public int MenuItemId { get; set; }

        // Snapshot of the menu item name at order time
        // Important: if the restaurant later renames the item, the order history stays accurate
        public string MenuItemName { get; set; } = string.Empty;

        // Snapshot of the price at order time — price may change later in the menu
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        // Optional field for future use (e.g., "extra spicy", "no onions")
        public string? Customizations { get; set; }

        // Navigation property — allows EF Core to load the parent Order
        // Nullable because it's not always needed (lazy loading)
        public Order? Order { get; set; }
    }
}
