using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class FAQ
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Question { get; set; } = string.Empty;
        
        [Required]
        public string Answer { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Category { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int DisplayOrder { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
    }
}
