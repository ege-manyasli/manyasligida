using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; }

        [Required]
        [StringLength(500)]
        public string ShippingAddress { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(10)]
        public string PostalCode { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed

        [StringLength(1000)]
        public string Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; }
    }
} 