using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models
{
    public class UserSession
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string SessionId { get; set; } = string.Empty;

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(100)]
        public string? DeviceInfo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastActivity { get; set; } = DateTime.Now;

        public DateTime? ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
