using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [Required(ErrorMessage = "Müşteri adı gereklidir")]
        [StringLength(100, ErrorMessage = "Müşteri adı en fazla 100 karakter olmalıdır")]
        [Display(Name = "Müşteri Adı")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olmalıdır")]
        [Display(Name = "E-posta")]
        public string CustomerEmail { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Telefon gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olmalıdır")]
        [Display(Name = "Telefon")]
        public string CustomerPhone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Teslimat adresi gereklidir")]
        [StringLength(500, ErrorMessage = "Teslimat adresi en fazla 500 karakter olmalıdır")]
        [Display(Name = "Teslimat Adresi")]
        public string ShippingAddress { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "Şehir en fazla 50 karakter olmalıdır")]
        [Display(Name = "Şehir")]
        public string? City { get; set; }
        
        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olmalıdır")]
        [Display(Name = "Posta Kodu")]
        public string? PostalCode { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Ara toplam 0 veya daha büyük olmalıdır")]
        [Display(Name = "Ara Toplam")]
        public decimal SubTotal { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "İndirim tutarı 0 veya daha büyük olmalıdır")]
        [Display(Name = "İndirim Tutarı")]
        public decimal DiscountAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Kargo ücreti 0 veya daha büyük olmalıdır")]
        [Display(Name = "Kargo Ücreti")]
        public decimal ShippingCost { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Vergi tutarı 0 veya daha büyük olmalıdır")]
        [Display(Name = "Vergi Tutarı")]
        public decimal TaxAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Toplam tutar 0 veya daha büyük olmalıdır")]
        [Display(Name = "Toplam Tutar")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Sipariş Durumu")]
        public string OrderStatus { get; set; } = "Pending";
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Ödeme Durumu")]
        public string PaymentStatus { get; set; } = "Pending";
        
        [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olmalıdır")]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
        
        [Display(Name = "Sipariş Tarihi")]
        public DateTime OrderDate { get; set; } = DateTimeHelper.NowTurkey;
        
        [Display(Name = "Gönderim Tarihi")]
        public DateTime? ShippedDate { get; set; }
        
        [Display(Name = "Teslimat Tarihi")]
        public DateTime? DeliveredDate { get; set; }
        
        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 