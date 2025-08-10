using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad gereklidir")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olmalıdır")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Soyad gereklidir")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olmalıdır")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olmalıdır")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Telefon gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olmalıdır")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Şifre gereklidir")]
        public string Password { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olmalıdır")]
        public string? Address { get; set; }
        
        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olmalıdır")]
        public string? City { get; set; }
        
        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olmalıdır")]
        public string? PostalCode { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        public bool EmailConfirmed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        
        // Google OAuth
        public string? GoogleId { get; set; }
        
        // Navigation properties
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        
        // Computed property
        public string FullName => $"{FirstName} {LastName}";
    }
} 