using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Video başlığı gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Video URL'si gereklidir")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz")]
        public string VideoUrl { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Thumbnail URL en fazla 200 karakter olabilir")]
        public string? ThumbnailUrl { get; set; }

        public int Duration { get; set; } = 0; // Saniye cinsinden süre

        public int ViewCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsFeatured { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}