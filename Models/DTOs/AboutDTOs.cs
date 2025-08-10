using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models.DTOs
{
    // Admin için About Edit DTO
    public record AboutEditRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; init; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Subtitle { get; init; } = string.Empty;

        [Required]
        public string StoryTitle { get; init; } = string.Empty;

        public string? StorySubtitle { get; init; }

        [Required]
        public string StoryContent { get; init; } = string.Empty;

        public string? StoryImageUrl { get; init; }

        [Required]
        public string MissionTitle { get; init; } = string.Empty;

        [Required]
        public string MissionContent { get; init; } = string.Empty;

        [Required]
        public string VisionTitle { get; init; } = string.Empty;

        [Required]
        public string VisionContent { get; init; } = string.Empty;

        public string? ValuesTitle { get; init; }
        public string? ValuesSubtitle { get; init; }
        public string? ValuesContent { get; init; }
        public List<ValueItemRequest> ValueItems { get; init; } = new();

        public string? ProductionTitle { get; init; }
        public string? ProductionSubtitle { get; init; }
        public List<ProductionStepRequest> ProductionSteps { get; init; } = new();

        public string? CertificatesTitle { get; init; }
        public string? CertificatesSubtitle { get; init; }
        public List<CertificateItemRequest> CertificateItems { get; init; } = new();

        public string? RegionTitle { get; init; }
        public string? RegionSubtitle { get; init; }
        public string? RegionContent { get; init; }
        public string? RegionImageUrl { get; init; }
        public List<RegionFeatureRequest> RegionFeatures { get; init; } = new();

        public string? CtaTitle { get; init; }
        public string? CtaContent { get; init; }
        public string? CtaButtonText { get; init; }
        public string? CtaSecondButtonText { get; init; }

        public List<StoryFeatureRequest> StoryFeatures { get; init; } = new();
    }

    public record ValueItemRequest
    {
        [Required]
        public string Title { get; init; } = string.Empty;
        [Required]
        public string Content { get; init; } = string.Empty;
        [Required]
        public string Icon { get; init; } = string.Empty; // FontAwesome class
        public string Color { get; init; } = "primary"; // Bootstrap color
    }

    public record ProductionStepRequest
    {
        public int StepNumber { get; init; }
        [Required]
        public string Title { get; init; } = string.Empty;
        [Required]
        public string Content { get; init; } = string.Empty;
    }

    public record CertificateItemRequest
    {
        [Required]
        public string Title { get; init; } = string.Empty;
        [Required]
        public string Description { get; init; } = string.Empty;
        [Required]
        public string Icon { get; init; } = string.Empty;
        public string Color { get; init; } = "primary";
    }

    public record RegionFeatureRequest
    {
        [Required]
        public string Title { get; init; } = string.Empty;
        [Required]
        public string Icon { get; init; } = string.Empty;
    }

    public record StoryFeatureRequest
    {
        [Required]
        public string Title { get; init; } = string.Empty;
    }

    // Response DTOs
    public record AboutContentResponse
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string StoryTitle { get; init; } = string.Empty;
        public string? StorySubtitle { get; init; }
        public string StoryContent { get; init; } = string.Empty;
        public string? StoryImageUrl { get; init; }
        public string MissionTitle { get; init; } = string.Empty;
        public string MissionContent { get; init; } = string.Empty;
        public string VisionTitle { get; init; } = string.Empty;
        public string VisionContent { get; init; } = string.Empty;
        public string? ValuesTitle { get; init; }
        public string? ValuesSubtitle { get; init; }
        public string? ValuesContent { get; init; }
        public List<ValueItemRequest> ValueItems { get; init; } = new();
        public string? ProductionTitle { get; init; }
        public string? ProductionSubtitle { get; init; }
        public List<ProductionStepRequest> ProductionSteps { get; init; } = new();
        public string? CertificatesTitle { get; init; }
        public string? CertificatesSubtitle { get; init; }
        public List<CertificateItemRequest> CertificateItems { get; init; } = new();
        public string? RegionTitle { get; init; }
        public string? RegionSubtitle { get; init; }
        public string? RegionContent { get; init; }
        public string? RegionImageUrl { get; init; }
        public List<RegionFeatureRequest> RegionFeatures { get; init; } = new();
        public string? CtaTitle { get; init; }
        public string? CtaContent { get; init; }
        public string? CtaButtonText { get; init; }
        public string? CtaSecondButtonText { get; init; }
        public List<StoryFeatureRequest> StoryFeatures { get; init; } = new();
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    // API Response wrapper
    public record AboutServiceResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
        public List<string> Errors { get; init; } = new();
    }
}
