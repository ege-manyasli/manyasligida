using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class CookieConsent
    {
        public int Id { get; set; }
        
        [StringLength(50)]
        public string? SessionId { get; set; }
        
        public int? UserId { get; set; }
        public User? User { get; set; }
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime ConsentDate { get; set; } = DateTimeHelper.NowTurkey;
        
        public DateTime ExpiryDate { get; set; } = DateTimeHelper.NowTurkey.AddYears(1);
        
        public bool IsActive { get; set; } = true;
        
        public bool IsAccepted { get; set; }
        
        [StringLength(1000)]
        public string? Preferences { get; set; } // JSON string for detailed preferences
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        
        // Navigation property
        public ICollection<CookieConsentDetail> ConsentDetails { get; set; } = new List<CookieConsentDetail>();
    }
}
