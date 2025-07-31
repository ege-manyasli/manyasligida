namespace manyasligida.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public string? ImageUrl { get; set; }
        
        // Çoklu görsel desteği
        public string? ImageUrls { get; set; } // JSON formatında çoklu görsel URL'leri
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
        public DateTime CreatedAt { get; set; } = DateTime.Now;
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