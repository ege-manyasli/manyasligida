using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string Phone { get; set; } = "+90 266 123 45 67";
        
        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; } = "info@manyasligida.com";
        
        [Required(ErrorMessage = "Adres gereklidir")]
        public string Address { get; set; } = "17 Eylül, Hal Cd. No:6 D:8, 10200 Bandırma/Balıkesir";
        
        [Required(ErrorMessage = "Çalışma saatleri gereklidir")]
        public string WorkingHours { get; set; } = "Pzt-Cmt: 08:00-18:00";
        
        [Url(ErrorMessage = "Geçerli bir Facebook URL'si giriniz")]
        public string FacebookUrl { get; set; } = "#";
        
        [Url(ErrorMessage = "Geçerli bir Instagram URL'si giriniz")]
        public string InstagramUrl { get; set; } = "#";
        
        [Url(ErrorMessage = "Geçerli bir Twitter URL'si giriniz")]
        public string TwitterUrl { get; set; } = "#";
        
        [Url(ErrorMessage = "Geçerli bir YouTube URL'si giriniz")]
        public string YoutubeUrl { get; set; } = "#";
        
        [Required(ErrorMessage = "Site başlığı gereklidir")]
        public string SiteTitle { get; set; } = "Manyaslı Süt Ürünleri";
        
        [Required(ErrorMessage = "Site açıklaması gereklidir")]
        public string SiteDescription { get; set; } = "Kaliteli ve taze süt ürünleri";
        
        [Required(ErrorMessage = "Site anahtar kelimeleri gereklidir")]
        public string SiteKeywords { get; set; } = "süt, peynir, yoğurt, manyas, gıda";
        
        public string LogoUrl { get; set; } = "/logomanyasli.png";
        
        public string FaviconUrl { get; set; } = "/favicon.ico";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

