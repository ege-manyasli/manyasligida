using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Cart
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        public User? User { get; set; }
        
        [StringLength(50)]
        public string? SessionId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
} 