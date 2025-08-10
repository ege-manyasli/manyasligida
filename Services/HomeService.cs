using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using System.Text.Json;

namespace manyasligida.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeService> _logger;

        public HomeService(ApplicationDbContext context, ILogger<HomeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HomeServiceResponse<HomeContentResponse>> GetHomeContentAsync()
        {
            try
            {
                _logger.LogInformation("Getting home content from database");
                
                // Check if HomeContents table exists
                var tableExists = 0;
                try
                {
                    tableExists = await _context.Database.SqlQueryRaw<int>(
                        "SELECT COUNT(*) as [Value] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HomeContents'"
                    ).FirstOrDefaultAsync();
                }
                catch (Exception tableCheckEx)
                {
                    _logger.LogWarning(tableCheckEx, "Could not check if HomeContents table exists");
                }
                
                if (tableExists == 0)
                {
                    _logger.LogWarning("HomeContents table does not exist");
                    return new HomeServiceResponse<HomeContentResponse>
                    {
                        Success = false,
                        Message = "HomeContents table does not exist. Please run the database script.",
                        Errors = new List<string> { "Database table missing" }
                    };
                }

                var homeContent = await _context.HomeContents
                    .Where(h => h.IsActive)
                    .OrderByDescending(h => h.UpdatedAt)
                    .FirstOrDefaultAsync();

                _logger.LogInformation("Found home content: {Found}", homeContent != null);

                if (homeContent == null)
                {
                    _logger.LogInformation("No home content found, creating default");
                    // Create default content if none exists
                    var defaultContent = await CreateDefaultHomeContentAsync();
                    return defaultContent;
                }

                var response = MapToResponse(homeContent);
                _logger.LogInformation("Successfully retrieved home content with VideoUrl: {VideoUrl}", response.HeroVideoUrl);
                
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = true,
                    Message = "Home content retrieved successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting home content - Exception: {Message}", ex.Message);
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = false,
                    Message = $"Error retrieving home content: {ex.Message}",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<HomeServiceResponse<HomeContentResponse>> UpdateHomeContentAsync(HomeEditRequest request)
        {
            try
            {
                var existingContent = await _context.HomeContents
                    .Where(h => h.IsActive)
                    .FirstOrDefaultAsync();

                if (existingContent == null)
                {
                    // Create new content
                    var newContent = MapFromRequest(request);
                    _context.HomeContents.Add(newContent);
                }
                else
                {
                    // Update existing content
                    UpdateExistingContent(existingContent, request);
                }

                await _context.SaveChangesAsync();

                var updatedContent = await _context.HomeContents
                    .Where(h => h.IsActive)
                    .OrderByDescending(h => h.UpdatedAt)
                    .FirstOrDefaultAsync();

                var response = MapToResponse(updatedContent!);

                _logger.LogInformation("Home content updated successfully");
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = true,
                    Message = "Home content updated successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating home content");
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = false,
                    Message = "Error updating home content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<HomeServiceResponse<HomeContentResponse>> CreateDefaultHomeContentAsync()
        {
            try
            {
                var defaultContent = new HomeContent
                {
                    HeroTitle = "Kaliteli Lezzetin Adresi",
                    HeroSubtitle = "Uzman ellerden sofralarınıza uzanan kaliteli süt ürünleri",
                    HeroDescription = "Taze ve güvenilir üretimle sağlığınız için en iyisi.",
                    HeroVideoUrl = "~/video/9586240-uhd_4096_2160_25fps.mp4",
                    HeroImageUrl = "~/img/manyasli-gida.png",
                    HeroButtonText = "Ürünleri Keşfet",
                    HeroSecondButtonText = "Hakkımızda",
                    
                    FeaturesTitle = "Neden Bizi Tercih Etmelisiniz?",
                    FeaturesSubtitle = "Kalite, güven ve lezzet bir arada",
                    FeatureItems = JsonSerializer.Serialize(GetDefaultFeatures()),
                    
                    ProductsTitle = "Popüler Ürünlerimiz",
                    ProductsSubtitle = "En çok tercih edilen lezzetler",
                    ShowPopularProducts = true,
                    MaxProductsToShow = 8,
                    
                    StatsTitle = "Güvenin Rakamları",
                    StatsSubtitle = "38 yıldır devam eden kalite yolculuğumuz",
                    StatsItems = JsonSerializer.Serialize(GetDefaultStats()),
                    
                    BlogTitle = "Son Haberler & Blog",
                    BlogSubtitle = "Sektörden son gelişmeler ve öneriler",
                    ShowLatestBlogs = true,
                    MaxBlogsToShow = 3,
                    
                    NewsletterTitle = "Haberdar Ol!",
                    NewsletterDescription = "Yeni ürünler, kampanyalar ve özel fırsatlardan ilk sen haberdar ol.",
                    NewsletterButtonText = "Abone Ol",
                    
                    ContactTitle = "İletişim",
                    ContactSubtitle = "Bizimle İletişime Geçin",
                    ContactDescription = "Sorularınız için bize ulaşabilir, özel siparişlerinizi verebilirsiniz.",
                    ContactPhone = "+90 266 123 45 67",
                    ContactEmail = "info@manyasligida.com",
                    ContactAddress = "Manyas, Balıkesir, Türkiye",
                    
                    HeroBackgroundColor = "linear-gradient(135deg, var(--primary-color) 0%, var(--secondary-color) 100%)",
                    PrimaryColor = "#8B4513",
                    SecondaryColor = "#D2691E",
                    
                    IsActive = true,
                    CreatedAt = DateTimeHelper.NowTurkey,
                    UpdatedAt = DateTimeHelper.NowTurkey
                };

                _context.HomeContents.Add(defaultContent);
                await _context.SaveChangesAsync();

                var response = MapToResponse(defaultContent);
                
                _logger.LogInformation("Default home content created");
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = true,
                    Message = "Default home content created",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default home content");
                return new HomeServiceResponse<HomeContentResponse>
                {
                    Success = false,
                    Message = "Error creating default home content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<HomeServiceResponse<bool>> DeleteHomeContentAsync(int id)
        {
            try
            {
                var content = await _context.HomeContents.FindAsync(id);
                if (content == null)
                {
                    return new HomeServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Home content not found"
                    };
                }

                content.IsActive = false;
                content.UpdatedAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Home content deleted: {Id}", id);
                return new HomeServiceResponse<bool>
                {
                    Success = true,
                    Message = "Home content deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting home content: {Id}", id);
                return new HomeServiceResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting home content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<HomeServiceResponse<List<HomeContentResponse>>> GetAllHomeContentsAsync()
        {
            try
            {
                var contents = await _context.HomeContents
                    .OrderByDescending(h => h.UpdatedAt)
                    .ToListAsync();

                var responses = contents.Select(MapToResponse).ToList();

                return new HomeServiceResponse<List<HomeContentResponse>>
                {
                    Success = true,
                    Message = "Home contents retrieved successfully",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all home contents");
                return new HomeServiceResponse<List<HomeContentResponse>>
                {
                    Success = false,
                    Message = "Error retrieving home contents",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Helper Methods
        private HomeContentResponse MapToResponse(HomeContent content)
        {
            return new HomeContentResponse
            {
                Id = content.Id,
                HeroTitle = content.HeroTitle,
                HeroSubtitle = content.HeroSubtitle,
                HeroDescription = content.HeroDescription,
                HeroVideoUrl = content.HeroVideoUrl,
                HeroImageUrl = content.HeroImageUrl,
                HeroButtonText = content.HeroButtonText,
                HeroButtonUrl = content.HeroButtonUrl,
                HeroSecondButtonText = content.HeroSecondButtonText,
                FeaturesTitle = content.FeaturesTitle,
                FeaturesSubtitle = content.FeaturesSubtitle,
                FeatureItems = DeserializeFeatures(content.FeatureItems),
                ProductsTitle = content.ProductsTitle,
                ProductsSubtitle = content.ProductsSubtitle,
                ShowPopularProducts = content.ShowPopularProducts,
                MaxProductsToShow = content.MaxProductsToShow,
                AboutTitle = content.AboutTitle,
                AboutSubtitle = content.AboutSubtitle,
                AboutContent = content.AboutContent,
                AboutDescription = content.AboutDescription,
                AboutImageUrl = content.AboutImageUrl,
                AboutButtonText = content.AboutButtonText,
                AboutFeatures = DeserializeAboutFeatures(content.AboutFeatures),
                ServicesTitle = content.ServicesTitle,
                ServicesSubtitle = content.ServicesSubtitle,
                ServicesDescription = content.ServicesDescription,
                ContactTitle = content.ContactTitle,
                ContactSubtitle = content.ContactSubtitle,
                ContactDescription = content.ContactDescription,
                ContactPhone = content.ContactPhone,
                ContactEmail = content.ContactEmail,
                ContactAddress = content.ContactAddress,
                StatsTitle = content.StatsTitle,
                StatsSubtitle = content.StatsSubtitle,
                StatsItems = DeserializeStats(content.StatsItems),
                BlogTitle = content.BlogTitle,
                BlogSubtitle = content.BlogSubtitle,
                ShowLatestBlogs = content.ShowLatestBlogs,
                MaxBlogsToShow = content.MaxBlogsToShow,
                NewsletterTitle = content.NewsletterTitle,
                NewsletterDescription = content.NewsletterDescription,
                NewsletterButtonText = content.NewsletterButtonText,
                HeroBackgroundColor = content.HeroBackgroundColor,
                PrimaryColor = content.PrimaryColor,
                SecondaryColor = content.SecondaryColor,
                IsActive = content.IsActive,
                CreatedAt = content.CreatedAt,
                UpdatedAt = content.UpdatedAt
            };
        }

        private HomeContent MapFromRequest(HomeEditRequest request)
        {
            return new HomeContent
            {
                HeroTitle = request.HeroTitle,
                HeroSubtitle = request.HeroSubtitle,
                HeroDescription = request.HeroDescription,
                HeroVideoUrl = request.HeroVideoUrl,
                HeroImageUrl = request.HeroImageUrl,
                HeroButtonText = request.HeroButtonText,
                HeroButtonUrl = request.HeroButtonUrl,
                HeroSecondButtonText = request.HeroSecondButtonText,
                FeaturesTitle = request.FeaturesTitle,
                FeaturesSubtitle = request.FeaturesSubtitle,
                FeatureItems = JsonSerializer.Serialize(request.FeatureItems),
                ProductsTitle = request.ProductsTitle,
                ProductsSubtitle = request.ProductsSubtitle,
                ShowPopularProducts = request.ShowPopularProducts,
                MaxProductsToShow = request.MaxProductsToShow,
                AboutTitle = request.AboutTitle,
                AboutSubtitle = request.AboutSubtitle,
                AboutContent = request.AboutContent,
                AboutDescription = request.AboutDescription,
                AboutImageUrl = request.AboutImageUrl,
                AboutButtonText = request.AboutButtonText,
                AboutFeatures = JsonSerializer.Serialize(request.AboutFeatures),
                ServicesTitle = request.ServicesTitle,
                ServicesSubtitle = request.ServicesSubtitle,
                ServicesDescription = request.ServicesDescription,
                ContactTitle = request.ContactTitle,
                ContactSubtitle = request.ContactSubtitle,
                ContactDescription = request.ContactDescription,
                ContactPhone = request.ContactPhone,
                ContactEmail = request.ContactEmail,
                ContactAddress = request.ContactAddress,
                StatsTitle = request.StatsTitle,
                StatsSubtitle = request.StatsSubtitle,
                StatsItems = JsonSerializer.Serialize(request.StatsItems),
                BlogTitle = request.BlogTitle,
                BlogSubtitle = request.BlogSubtitle,
                ShowLatestBlogs = request.ShowLatestBlogs,
                MaxBlogsToShow = request.MaxBlogsToShow,
                NewsletterTitle = request.NewsletterTitle,
                NewsletterDescription = request.NewsletterDescription,
                NewsletterButtonText = request.NewsletterButtonText,
                HeroBackgroundColor = request.HeroBackgroundColor,
                PrimaryColor = request.PrimaryColor,
                SecondaryColor = request.SecondaryColor,
                IsActive = true,
                CreatedAt = DateTimeHelper.NowTurkey,
                UpdatedAt = DateTimeHelper.NowTurkey
            };
        }

        private void UpdateExistingContent(HomeContent existing, HomeEditRequest request)
        {
            existing.HeroTitle = request.HeroTitle;
            existing.HeroSubtitle = request.HeroSubtitle;
            existing.HeroDescription = request.HeroDescription;
            existing.HeroVideoUrl = request.HeroVideoUrl;
            existing.HeroImageUrl = request.HeroImageUrl;
            existing.HeroButtonText = request.HeroButtonText;
            existing.HeroButtonUrl = request.HeroButtonUrl;
            existing.HeroSecondButtonText = request.HeroSecondButtonText;
            existing.FeaturesTitle = request.FeaturesTitle;
            existing.FeaturesSubtitle = request.FeaturesSubtitle;
            existing.FeatureItems = JsonSerializer.Serialize(request.FeatureItems);
            existing.ProductsTitle = request.ProductsTitle;
            existing.ProductsSubtitle = request.ProductsSubtitle;
            existing.ShowPopularProducts = request.ShowPopularProducts;
            existing.MaxProductsToShow = request.MaxProductsToShow;
            existing.AboutTitle = request.AboutTitle;
            existing.AboutSubtitle = request.AboutSubtitle;
            existing.AboutContent = request.AboutContent;
            existing.AboutDescription = request.AboutDescription;
            existing.AboutImageUrl = request.AboutImageUrl;
            existing.AboutButtonText = request.AboutButtonText;
            existing.AboutFeatures = JsonSerializer.Serialize(request.AboutFeatures);
            existing.ServicesTitle = request.ServicesTitle;
            existing.ServicesSubtitle = request.ServicesSubtitle;
            existing.ServicesDescription = request.ServicesDescription;
            existing.ContactTitle = request.ContactTitle;
            existing.ContactSubtitle = request.ContactSubtitle;
            existing.ContactDescription = request.ContactDescription;
            existing.ContactPhone = request.ContactPhone;
            existing.ContactEmail = request.ContactEmail;
            existing.ContactAddress = request.ContactAddress;
            existing.StatsTitle = request.StatsTitle;
            existing.StatsSubtitle = request.StatsSubtitle;
            existing.StatsItems = JsonSerializer.Serialize(request.StatsItems);
            existing.BlogTitle = request.BlogTitle;
            existing.BlogSubtitle = request.BlogSubtitle;
            existing.ShowLatestBlogs = request.ShowLatestBlogs;
            existing.MaxBlogsToShow = request.MaxBlogsToShow;
            existing.NewsletterTitle = request.NewsletterTitle;
            existing.NewsletterDescription = request.NewsletterDescription;
            existing.NewsletterButtonText = request.NewsletterButtonText;
            existing.HeroBackgroundColor = request.HeroBackgroundColor;
            existing.PrimaryColor = request.PrimaryColor;
            existing.SecondaryColor = request.SecondaryColor;
            existing.UpdatedAt = DateTimeHelper.NowTurkey;
        }

        // JSON Deserialization helpers
        private List<FeatureItemRequest> DeserializeFeatures(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultFeatures();
            try
            {
                return JsonSerializer.Deserialize<List<FeatureItemRequest>>(json) ?? GetDefaultFeatures();
            }
            catch
            {
                return GetDefaultFeatures();
            }
        }

        private List<AboutFeatureRequest> DeserializeAboutFeatures(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultAboutFeatures();
            try
            {
                return JsonSerializer.Deserialize<List<AboutFeatureRequest>>(json) ?? GetDefaultAboutFeatures();
            }
            catch
            {
                return GetDefaultAboutFeatures();
            }
        }

        private List<StatsItemRequest> DeserializeStats(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultStats();
            try
            {
                return JsonSerializer.Deserialize<List<StatsItemRequest>>(json) ?? GetDefaultStats();
            }
            catch
            {
                return GetDefaultStats();
            }
        }

        // Default data providers
        private List<FeatureItemRequest> GetDefaultFeatures()
        {
            return new List<FeatureItemRequest>
            {
                new() { Title = "Kaliteli Üretim", Description = "Modern tesislerde hijyenik koşullarda üretim", Icon = "fas fa-award", Color = "primary" },
                new() { Title = "Taze Ürünler", Description = "Her gün taze olarak üretilen süt ürünleri", Icon = "fas fa-leaf", Color = "success" },
                new() { Title = "Güvenilir Marka", Description = "38 yıldır süren güven ve kalite anlayışı", Icon = "fas fa-shield-alt", Color = "info" },
                new() { Title = "Hızlı Teslimat", Description = "Siparişlerinizi hızlı ve güvenli şekilde teslim ediyoruz", Icon = "fas fa-shipping-fast", Color = "warning" }
            };
        }

        private List<AboutFeatureRequest> GetDefaultAboutFeatures()
        {
            return new List<AboutFeatureRequest>
            {
                new() { Title = "Deneyim", Value = "38+ Yıl" },
                new() { Title = "Müşteri", Value = "50.000+" },
                new() { Title = "Ürün Çeşidi", Value = "100+" },
                new() { Title = "İl", Value = "25+" }
            };
        }

        private List<StatsItemRequest> GetDefaultStats()
        {
            return new List<StatsItemRequest>
            {
                new() { Title = "Mutlu Müşteri", Value = "50000", Icon = "fas fa-users", Suffix = "+", Color = "primary" },
                new() { Title = "Yıllık Deneyim", Value = "38", Icon = "fas fa-calendar", Suffix = "+", Color = "success" },
                new() { Title = "Ürün Çeşidi", Value = "100", Icon = "fas fa-boxes", Suffix = "+", Color = "info" },
                new() { Title = "Şehir", Value = "25", Icon = "fas fa-map-marker-alt", Suffix = "+", Color = "warning" }
            };
        }
    }
}
