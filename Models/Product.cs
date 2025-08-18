using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir")]
        [StringLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olmalıdır")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olmalıdır")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Temporarily commented out until migration is applied
        // [StringLength(200, ErrorMessage = "Kısa açıklama en fazla 200 karakter olmalıdır")]
        // [Display(Name = "Kısa Açıklama")]
        // public string? ShortDescription { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Eski fiyat 0'dan büyük olmalıdır")]
        [Display(Name = "Eski Fiyat")]
        public decimal? OldPrice { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Görsel URL")]
        public string? ImageUrl { get; set; }

        // Çoklu görsel desteği
        public string? ImageUrls { get; set; } // JSON formatında çoklu görsel URL'leri
        public string? GalleryImageUrls { get; set; } // Galeri görselleri için JSON formatında URL'ler
        public string? ThumbnailUrl { get; set; }

        // Ürün özellikleri
        public bool IsPopular { get; set; }
        public bool IsNew { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        
        // Ürün detayları
        public string? Weight { get; set; }
        public string? FatContent { get; set; }
        public string? Ingredients { get; set; }
        public string? NutritionalInfo { get; set; }
        public string? StorageInfo { get; set; }
        public string? ExpiryInfo { get; set; }
        public string? AllergenInfo { get; set; }

        // SEO ve meta bilgileri
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaKeywords { get; set; }
        public string? Slug { get; set; }

        // Zaman damgaları
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Navigation properties
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Yardımcı metodlar
        public List<string> GetImageUrls()
        {
            if (string.IsNullOrEmpty(ImageUrls))
                return new List<string> { ImageUrl ?? "" };

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImageUrls) ?? new List<string>();
            }
            catch
            {
                return new List<string> { ImageUrl ?? "" };
            }
        }

        public void SetImageUrls(List<string> urls)
        {
            ImageUrls = System.Text.Json.JsonSerializer.Serialize(urls);
        }

        public List<string> GetGalleryImageUrls()
        {
            if (string.IsNullOrEmpty(GalleryImageUrls))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(GalleryImageUrls) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void SetGalleryImageUrls(List<string> urls)
        {
            GalleryImageUrls = System.Text.Json.JsonSerializer.Serialize(urls);
        }

        public string GetMainImageUrl()
        {
            var urls = GetImageUrls();
            return urls.FirstOrDefault() ?? ImageUrl ?? "";
        }

        public bool HasDiscount => OldPrice.HasValue && OldPrice > Price;

        public decimal DiscountPercentage
        {
            get
            {
                if (!HasDiscount) return 0;
                return Math.Round(((OldPrice!.Value - Price) / OldPrice.Value) * 100, 0);
            }
        }

        public bool IsInStock => StockQuantity > 0;

        public string StockStatus => IsInStock ? "Stokta" : "Stokta Yok";
    }
}