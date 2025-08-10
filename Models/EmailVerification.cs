using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class EmailVerification
    {
        public int Id { get; set; }
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string VerificationCode { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
    }
}
