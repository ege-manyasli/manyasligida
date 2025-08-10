using manyasligida.Services;

namespace manyasligida.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsReplied { get; set; } = false;
        public string? ReplyMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? RepliedAt { get; set; }
    }
} 