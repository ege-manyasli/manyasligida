using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class CookieCategory
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public bool IsRequired { get; set; } = false;
        
        public bool IsActive { get; set; } = true;
        
        public int SortOrder { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public ICollection<CookieConsentDetail> ConsentDetails { get; set; } = new List<CookieConsentDetail>();
    }
}
