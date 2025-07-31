using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(100)]
        public string City { get; set; }

        [StringLength(10)]
        public string PostalCode { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
        public ICollection<Cart> Carts { get; set; }

        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
} 