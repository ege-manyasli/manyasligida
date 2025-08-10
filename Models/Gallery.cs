using manyasligida.Services;

namespace manyasligida.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime? UpdatedAt { get; set; }

        // EKLENEN SATIR
        public string? Category { get; set; }
    }
}
