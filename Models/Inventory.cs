using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Mal ismi gereklidir")]
        [StringLength(200, ErrorMessage = "Mal ismi en fazla 200 karakter olmalıdır")]
        [Display(Name = "Mal İsmi")]
        public string ItemName { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olmalıdır")]
        [Display(Name = "Kategori")]
        public string? Category { get; set; }
        
        [Required(ErrorMessage = "Geliş fiyatı gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Geliş fiyatı 0'dan büyük olmalıdır")]
        [Display(Name = "Geliş Fiyatı (₺)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        
        [Required(ErrorMessage = "Miktar gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır")]
        [Display(Name = "Miktar")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }
        
        [StringLength(20, ErrorMessage = "Birim en fazla 20 karakter olmalıdır")]
        [Display(Name = "Birim")]
        public string? Unit { get; set; } // kg, lt, adet, paket
        
        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount => Math.Round(UnitPrice * Quantity, 2);
        
        [StringLength(200, ErrorMessage = "Tedarikçi en fazla 200 karakter olmalıdır")]
        [Display(Name = "Tedarikçi")]
        public string? Supplier { get; set; }
        
        [Required(ErrorMessage = "Tarih gereklidir")]
        [Display(Name = "Alım Tarihi")]
        public DateTime PurchaseDate { get; set; } = DateTimeHelper.NowTurkey;
        
        [StringLength(500, ErrorMessage = "Notlar en fazla 500 karakter olmalıdır")]
        [Display(Name = "Notlar")]
        public string? Notes { get; set; }
        
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        
        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
    
    public class InventoryStock
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Mal ismi gereklidir")]
        [StringLength(200, ErrorMessage = "Mal ismi en fazla 200 karakter olmalıdır")]
        [Display(Name = "Mal İsmi")]
        public string ItemName { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olmalıdır")]
        [Display(Name = "Kategori")]
        public string? Category { get; set; }
        
        [Display(Name = "Mevcut Stok")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal CurrentStock { get; set; } = 0;
        
        [Display(Name = "Minimum Stok")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal MinimumStock { get; set; } = 0;
        
        [StringLength(20, ErrorMessage = "Birim en fazla 20 karakter olmalıdır")]
        [Display(Name = "Birim")]
        public string? Unit { get; set; }
        
        [Display(Name = "Ortalama Maliyet")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AverageCost { get; set; } = 0;
        
        [Display(Name = "Stok Değeri")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StockValue => Math.Round(CurrentStock * AverageCost, 2);
        
        [Display(Name = "Son Güncelleme")]
        public DateTime UpdatedAt { get; set; } = DateTimeHelper.NowTurkey;
        
        [Display(Name = "Son Güncelleme")]
        public DateTime LastUpdated { get; set; } = DateTimeHelper.NowTurkey;
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
    
    public class InventoryTransaction
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Mal ismi gereklidir")]
        [StringLength(200, ErrorMessage = "Mal ismi en fazla 200 karakter olmalıdır")]
        [Display(Name = "Mal İsmi")]
        public string ItemName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "İşlem tipi gereklidir")]
        [StringLength(20, ErrorMessage = "İşlem tipi en fazla 20 karakter olmalıdır")]
        [Display(Name = "İşlem Tipi")]
        public string TransactionType { get; set; } = string.Empty; // IN, OUT
        
        [Required(ErrorMessage = "Miktar gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar 0'dan büyük olmalıdır")]
        [Display(Name = "Miktar")]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }
        
        [StringLength(20, ErrorMessage = "Birim en fazla 20 karakter olmalıdır")]
        [Display(Name = "Birim")]
        public string? Unit { get; set; }
        
        [Display(Name = "Birim Fiyat")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0;
        
        [Display(Name = "Toplam Tutar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount => Math.Round(UnitPrice * Quantity, 2);
        
        [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olmalıdır")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
        
        [Display(Name = "İşlem Tarihi")]
        public DateTime TransactionDate { get; set; } = DateTimeHelper.NowTurkey;
        
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
    }
}
