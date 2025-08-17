using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class Blog
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olmalıdır")]
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Özet en fazla 500 karakter olmalıdır")]
        [Display(Name = "Özet")]
        public string? Summary { get; set; }
        
        [Required(ErrorMessage = "İçerik gereklidir")]
        [Display(Name = "İçerik")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Görsel URL")]
        public string? ImageUrl { get; set; }
        
        [StringLength(100, ErrorMessage = "Yazar adı en fazla 100 karakter olmalıdır")]
        [Display(Name = "Yazar")]
        public string? Author { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }
        
        [Display(Name = "Yayın Tarihi")]
        public DateTime? PublishedAt { get; set; }
    }
} 