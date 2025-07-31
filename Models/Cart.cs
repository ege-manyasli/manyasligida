using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<CartItem> CartItems { get; set; }

        // Computed properties
        public int TotalItems => CartItems?.Sum(ci => ci.Quantity) ?? 0;
        public decimal TotalAmount => CartItems?.Sum(ci => ci.TotalPrice) ?? 0;
    }
} 