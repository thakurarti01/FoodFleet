namespace RestaurantService.Models
{
    public class Restaurant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CuisineTypes { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string ApprovalStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? RejectionReason { get; set; }
        public bool IsOpen { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
