using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class UserSession
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        [StringLength(45)]
        public string? IpAddress { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [StringLength(100)]
        public string? DeviceInfo { get; set; }
        
        [StringLength(50)]
        public string? Location { get; set; }
    }
}
