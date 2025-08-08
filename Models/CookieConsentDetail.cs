using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class CookieConsentDetail
    {
        public int Id { get; set; }
        
        public int CookieConsentId { get; set; }
        public CookieConsent CookieConsent { get; set; } = null!;
        
        public int CookieCategoryId { get; set; }
        public CookieCategory CookieCategory { get; set; } = null!;
        
        public bool IsAccepted { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
