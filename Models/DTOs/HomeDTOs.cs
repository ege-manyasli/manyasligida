using System.ComponentModel.DataAnnotations;

namespace manyasligida.Models.DTOs
{
    // Home Edit Request DTO
    public record HomeEditRequest
    {
        // HERO SECTION
        [Required]
        [StringLength(200)]
        public string HeroTitle { get; init; } = string.Empty;

        [StringLength(500)]
        public string? HeroSubtitle { get; init; }

        public string? HeroDescription { get; init; }

        public string? HeroVideoUrl { get; init; }

        public string? HeroImageUrl { get; init; }

        public string? HeroButtonText { get; init; }

        public string? HeroButtonUrl { get; init; }

        public string? HeroSecondButtonText { get; init; }

        // FEATURES SECTION
        public string? FeaturesTitle { get; init; }
        public string? FeaturesSubtitle { get; init; }
        public List<FeatureItemRequest> FeatureItems { get; init; } = new();

        // POPULAR PRODUCTS SECTION
        public string? ProductsTitle { get; init; }
        public string? ProductsSubtitle { get; init; }
        public bool ShowPopularProducts { get; init; } = true;
        public int MaxProductsToShow { get; init; } = 8;

        // ABOUT SECTION
        public string? AboutTitle { get; init; }
        public string? AboutSubtitle { get; init; }
        public string? AboutContent { get; init; }
        public string? AboutImageUrl { get; init; }
        public string? AboutButtonText { get; init; }
        public List<AboutFeatureRequest> AboutFeatures { get; init; } = new();

        // SERVICES SECTION
        public string? ServicesTitle { get; init; }
        public string? ServicesSubtitle { get; init; }
        public string? ServicesDescription { get; init; }

        // CONTACT SECTION
        public string? ContactTitle { get; init; }
        public string? ContactSubtitle { get; init; }
        public string? ContactDescription { get; init; }
        public string? ContactPhone { get; init; }
        public string? ContactEmail { get; init; }
        public string? ContactAddress { get; init; }

        // STATS SECTION
        public string? StatsTitle { get; init; }
        public string? StatsSubtitle { get; init; }
        public List<StatsItemRequest> StatsItems { get; init; } = new();

        // BLOG SECTION
        public string? BlogTitle { get; init; }
        public string? BlogSubtitle { get; init; }
        public bool ShowLatestBlogs { get; init; } = true;
        public int MaxBlogsToShow { get; init; } = 3;

        // NEWSLETTER SECTION
        public string? NewsletterTitle { get; init; }
        public string? NewsletterDescription { get; init; }
        public string? NewsletterButtonText { get; init; }

        // STYLING & COLORS
        public string? HeroBackgroundColor { get; init; }
        public string? PrimaryColor { get; init; }
        public string? SecondaryColor { get; init; }
    }

    // Sub-DTOs for complex sections
    public record FeatureItemRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty; // FontAwesome class
        public string Color { get; init; } = "primary";
    }

    public record AboutFeatureRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
    }

    public record StatsItemRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;
        public string Suffix { get; init; } = string.Empty; // %, +, K, etc.
        public string Color { get; init; } = "primary";
    }

    // Response DTO
    public record HomeContentResponse
    {
        public int Id { get; init; }

        // HERO SECTION
        public string HeroTitle { get; init; } = string.Empty;
        public string? HeroSubtitle { get; init; }
        public string? HeroDescription { get; init; }
        public string? HeroVideoUrl { get; init; }
        public string? HeroImageUrl { get; init; }
        public string? HeroButtonText { get; init; }
        public string? HeroButtonUrl { get; init; }
        public string? HeroSecondButtonText { get; init; }

        // FEATURES SECTION
        public string? FeaturesTitle { get; init; }
        public string? FeaturesSubtitle { get; init; }
        public List<FeatureItemRequest> FeatureItems { get; init; } = new();

        // POPULAR PRODUCTS SECTION
        public string? ProductsTitle { get; init; }
        public string? ProductsSubtitle { get; init; }
        public bool ShowPopularProducts { get; init; }
        public int MaxProductsToShow { get; init; }

        // ABOUT SECTION
        public string? AboutTitle { get; init; }
        public string? AboutSubtitle { get; init; }
        public string? AboutContent { get; init; }
        public string? AboutImageUrl { get; init; }
        public string? AboutButtonText { get; init; }
        public List<AboutFeatureRequest> AboutFeatures { get; init; } = new();

        // SERVICES SECTION
        public string? ServicesTitle { get; init; }
        public string? ServicesSubtitle { get; init; }
        public string? ServicesDescription { get; init; }

        // CONTACT SECTION
        public string? ContactTitle { get; init; }
        public string? ContactSubtitle { get; init; }
        public string? ContactDescription { get; init; }
        public string? ContactPhone { get; init; }
        public string? ContactEmail { get; init; }
        public string? ContactAddress { get; init; }

        // STATS SECTION
        public string? StatsTitle { get; init; }
        public string? StatsSubtitle { get; init; }
        public List<StatsItemRequest> StatsItems { get; init; } = new();

        // BLOG SECTION
        public string? BlogTitle { get; init; }
        public string? BlogSubtitle { get; init; }
        public bool ShowLatestBlogs { get; init; }
        public int MaxBlogsToShow { get; init; }

        // NEWSLETTER SECTION
        public string? NewsletterTitle { get; init; }
        public string? NewsletterDescription { get; init; }
        public string? NewsletterButtonText { get; init; }

        // STYLING & COLORS
        public string? HeroBackgroundColor { get; init; }
        public string? PrimaryColor { get; init; }
        public string? SecondaryColor { get; init; }

        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }

    // API Response wrapper
    public record HomeServiceResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
        public List<string> Errors { get; init; } = new();
    }
}
