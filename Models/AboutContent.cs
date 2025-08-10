using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class AboutContent
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subtitle { get; set; } = string.Empty;

        [Required]
        public string StoryTitle { get; set; } = string.Empty;

        public string? StorySubtitle { get; set; }

        [Required]
        public string StoryContent { get; set; } = string.Empty;

        public string? StoryImageUrl { get; set; }

        // Mission & Vision
        [Required]
        public string MissionTitle { get; set; } = string.Empty;

        [Required]
        public string MissionContent { get; set; } = string.Empty;

        [Required]
        public string VisionTitle { get; set; } = string.Empty;

        [Required]
        public string VisionContent { get; set; } = string.Empty;

        // Values Section
        public string? ValuesTitle { get; set; }
        public string? ValuesSubtitle { get; set; }
        public string? ValuesContent { get; set; }

        // Value Items (JSON formatında saklanacak)
        public string? ValueItems { get; set; } // JSON: [{"title":"", "content":"", "icon":""}]

        // Production Process
        public string? ProductionTitle { get; set; }
        public string? ProductionSubtitle { get; set; }
        public string? ProductionSteps { get; set; } // JSON format

        // Certificates
        public string? CertificatesTitle { get; set; }
        public string? CertificatesSubtitle { get; set; }
        public string? CertificateItems { get; set; } // JSON format

        // Regional Info
        public string? RegionTitle { get; set; }
        public string? RegionSubtitle { get; set; }
        public string? RegionContent { get; set; }
        public string? RegionImageUrl { get; set; }
        public string? RegionFeatures { get; set; } // JSON format

        // CTA Section
        public string? CtaTitle { get; set; }
        public string? CtaContent { get; set; }
        public string? CtaButtonText { get; set; }
        public string? CtaSecondButtonText { get; set; }

        // Display Features (like in story section)
        public string? StoryFeatures { get; set; } // JSON format for checkmarks

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime UpdatedAt { get; set; } = DateTimeHelper.NowTurkey;
    }
}
