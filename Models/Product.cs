using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OldPrice { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public bool IsPopular { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string Weight { get; set; }

        [StringLength(20)]
        public string FatContent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
} 