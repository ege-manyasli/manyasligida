using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Summary { get; set; }

        [Required]
        public string Content { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string Author { get; set; }

        public bool IsPublished { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PublishedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int ViewCount { get; set; } = 0;
    }
} 