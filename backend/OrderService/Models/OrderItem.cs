namespace OrderService.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = string.Empty; // snapshot
        public decimal Price { get; set; }                        // snapshot
        public int Quantity { get; set; }
        public string? Customizations { get; set; }

        public Order? Order { get; set; }
    }
}
