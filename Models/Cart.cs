using manyasligida.Services;

namespace manyasligida.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        
        // Computed properties
        public int TotalItems => Items?.Sum(ci => ci.Quantity) ?? 0;
        public decimal TotalAmount => Items?.Sum(ci => ci.TotalPrice) ?? 0;
    }
} 