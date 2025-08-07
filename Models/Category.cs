using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Category
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Kategori adı gereklidir")]
        [StringLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olmalıdır")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olmalıdır")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
        
        [Display(Name = "Görsel URL")]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        [Range(0, int.MaxValue, ErrorMessage = "Görüntüleme sırası 0 veya daha büyük olmalıdır")]
        [Display(Name = "Görüntüleme Sırası")]
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
} 