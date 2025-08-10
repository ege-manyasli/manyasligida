using System.ComponentModel.DataAnnotations;
using manyasligida.Services;

namespace manyasligida.Models
{
    public class HomeContent
    {
        public int Id { get; set; }

        // HERO SECTION
        [Required]
        [StringLength(200)]
        public string HeroTitle { get; set; } = string.Empty;

        [StringLength(500)]
        public string? HeroSubtitle { get; set; }

        public string? HeroDescription { get; set; }

        public string? HeroVideoUrl { get; set; }

        public string? HeroImageUrl { get; set; }

        public string? HeroButtonText { get; set; }

        public string? HeroSecondButtonText { get; set; }

        // FEATURES SECTION
        public string? FeaturesTitle { get; set; }
        public string? FeaturesSubtitle { get; set; }
        public string? FeatureItems { get; set; } // JSON: [{"title":"", "description":"", "icon":"", "color":""}]

        // POPULAR PRODUCTS SECTION
        public string? ProductsTitle { get; set; }
        public string? ProductsSubtitle { get; set; }
        public bool ShowPopularProducts { get; set; } = true;
        public int MaxProductsToShow { get; set; } = 8;

        // ABOUT SECTION
        public string? AboutTitle { get; set; }
        public string? AboutContent { get; set; }
        public string? AboutImageUrl { get; set; }
        public string? AboutButtonText { get; set; }
        public string? AboutFeatures { get; set; } // JSON: [{"title":"", "value":""}]

        // STATS SECTION
        public string? StatsTitle { get; set; }
        public string? StatsSubtitle { get; set; }
        public string? StatsItems { get; set; } // JSON: [{"title":"", "value":"", "icon":"", "suffix":""}]

        // BLOG SECTION
        public string? BlogTitle { get; set; }
        public string? BlogSubtitle { get; set; }
        public bool ShowLatestBlogs { get; set; } = true;
        public int MaxBlogsToShow { get; set; } = 3;

        // NEWSLETTER SECTION
        public string? NewsletterTitle { get; set; }
        public string? NewsletterDescription { get; set; }
        public string? NewsletterButtonText { get; set; }

        // STYLING & COLORS
        public string? HeroBackgroundColor { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }

        // META
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTimeHelper.NowTurkey;
        public DateTime UpdatedAt { get; set; } = DateTimeHelper.NowTurkey;
    }
}
