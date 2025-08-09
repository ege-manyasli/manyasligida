using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using System.Text.Json;

namespace manyasligida.Services
{
    public class AboutService : IAboutService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AboutService> _logger;

        public AboutService(ApplicationDbContext context, ILogger<AboutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AboutServiceResponse<AboutContentResponse>> GetAboutContentAsync()
        {
            try
            {
                var aboutContent = await _context.AboutContents
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.UpdatedAt)
                    .FirstOrDefaultAsync();

                if (aboutContent == null)
                {
                    // Create default content if none exists
                    var defaultContent = await CreateDefaultAboutContentAsync();
                    return defaultContent;
                }

                var response = MapToResponse(aboutContent);
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = true,
                    Message = "About content retrieved successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting about content");
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = false,
                    Message = "Error retrieving about content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AboutServiceResponse<AboutContentResponse>> UpdateAboutContentAsync(AboutEditRequest request)
        {
            try
            {
                var existingContent = await _context.AboutContents
                    .Where(a => a.IsActive)
                    .FirstOrDefaultAsync();

                if (existingContent == null)
                {
                    // Create new content
                    var newContent = MapFromRequest(request);
                    _context.AboutContents.Add(newContent);
                }
                else
                {
                    // Update existing content
                    UpdateExistingContent(existingContent, request);
                }

                await _context.SaveChangesAsync();

                var updatedContent = await _context.AboutContents
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.UpdatedAt)
                    .FirstOrDefaultAsync();

                var response = MapToResponse(updatedContent!);

                _logger.LogInformation("About content updated successfully");
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = true,
                    Message = "About content updated successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating about content");
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = false,
                    Message = "Error updating about content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AboutServiceResponse<AboutContentResponse>> CreateDefaultAboutContentAsync()
        {
            try
            {
                var defaultContent = new AboutContent
                {
                    Title = "Hakkımızda",
                    Subtitle = "Gelenekten geleceğe kaliteli lezzetlerin hikayesi",
                    StoryTitle = "Hikayemiz",
                    StoryContent = "1985 yılında Balıkesir Bandırma'da başlayan yolculuğumuz, bugün Türkiye'nin dört bir yanına ulaşan kaliteli lezzetlerle devam ediyor. Dedemizden öğrendiğimiz geleneksel üretim yöntemlerini modern teknoloji ile birleştirerek, sofralarınıza en taze ve kaliteli süt ürünlerini sunuyoruz.",
                    StoryImageUrl = "~/img/slider1-1920x750.jpeg",
                    MissionTitle = "Misyonumuz",
                    MissionContent = "Geleneksel lezzetleri koruyarak, modern üretim standartları ile birleştirip, müşterilerimize en kaliteli süt ürünlerini sunmak. Her bir ürünümüzde Balıkesir'in bereketli topraklarının lezzetini yaşatmak.",
                    VisionTitle = "Vizyonumuz", 
                    VisionContent = "Türkiye'nin en güvenilir ve tercih edilen kaliteli süt ürünleri markası olmak. Geleneksel lezzetleri koruyarak, yeni nesillere aktarmak ve uluslararası pazarlarda da tanınan bir marka haline gelmek.",
                    ValuesTitle = "Değerlerimiz",
                    ValuesSubtitle = "Bizi biz yapan temel değerler",
                    ValueItems = JsonSerializer.Serialize(GetDefaultValueItems()),
                    ProductionTitle = "Üretim Sürecimiz",
                    ProductionSubtitle = "Kaliteli lezzetlerin yolculuğu",
                    ProductionSteps = JsonSerializer.Serialize(GetDefaultProductionSteps()),
                    CertificatesTitle = "Sertifikalarımız",
                    CertificatesSubtitle = "Kalitemizin kanıtı",
                    CertificateItems = JsonSerializer.Serialize(GetDefaultCertificates()),
                    RegionTitle = "Balıkesir Bandırma Bölgesi",
                    RegionContent = "Bandırma, Balıkesir'in bereketli topraklarında yer alan, doğal güzellikleri ve temiz havası ile ünlü bir bölgedir. Bölgenin zengin otlakları ve temiz su kaynakları, ineklerimizin en kaliteli sütü üretmesini sağlar.",
                    RegionImageUrl = "~/img/slider2-1920x750.jpeg",
                    RegionFeatures = JsonSerializer.Serialize(GetDefaultRegionFeatures()),
                    CtaTitle = "Kaliteli Lezzetleri Keşfedin!",
                    CtaContent = "Balıkesir Bandırma'nın bereketli topraklarından sofralarınıza uzanan eşsiz lezzetleri deneyin.",
                    CtaButtonText = "Ürünlerimizi İnceleyin",
                    CtaSecondButtonText = "Bize Ulaşın",
                    StoryFeatures = JsonSerializer.Serialize(GetDefaultStoryFeatures()),
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.AboutContents.Add(defaultContent);
                await _context.SaveChangesAsync();

                var response = MapToResponse(defaultContent);
                
                _logger.LogInformation("Default about content created");
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = true,
                    Message = "Default about content created",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default about content");
                return new AboutServiceResponse<AboutContentResponse>
                {
                    Success = false,
                    Message = "Error creating default about content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AboutServiceResponse<bool>> DeleteAboutContentAsync(int id)
        {
            try
            {
                var content = await _context.AboutContents.FindAsync(id);
                if (content == null)
                {
                    return new AboutServiceResponse<bool>
                    {
                        Success = false,
                        Message = "About content not found"
                    };
                }

                content.IsActive = false;
                content.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("About content deleted: {Id}", id);
                return new AboutServiceResponse<bool>
                {
                    Success = true,
                    Message = "About content deleted successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting about content: {Id}", id);
                return new AboutServiceResponse<bool>
                {
                    Success = false,
                    Message = "Error deleting about content",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AboutServiceResponse<List<AboutContentResponse>>> GetAllAboutContentsAsync()
        {
            try
            {
                var contents = await _context.AboutContents
                    .OrderByDescending(a => a.UpdatedAt)
                    .ToListAsync();

                var responses = contents.Select(MapToResponse).ToList();

                return new AboutServiceResponse<List<AboutContentResponse>>
                {
                    Success = true,
                    Message = "About contents retrieved successfully",
                    Data = responses
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all about contents");
                return new AboutServiceResponse<List<AboutContentResponse>>
                {
                    Success = false,
                    Message = "Error retrieving about contents",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // Helper Methods
        private AboutContentResponse MapToResponse(AboutContent content)
        {
            return new AboutContentResponse
            {
                Id = content.Id,
                Title = content.Title,
                Subtitle = content.Subtitle,
                StoryTitle = content.StoryTitle,
                StoryContent = content.StoryContent,
                StoryImageUrl = content.StoryImageUrl,
                MissionTitle = content.MissionTitle,
                MissionContent = content.MissionContent,
                VisionTitle = content.VisionTitle,
                VisionContent = content.VisionContent,
                ValuesTitle = content.ValuesTitle,
                ValuesSubtitle = content.ValuesSubtitle,
                ValueItems = DeserializeValueItems(content.ValueItems),
                ProductionTitle = content.ProductionTitle,
                ProductionSubtitle = content.ProductionSubtitle,
                ProductionSteps = DeserializeProductionSteps(content.ProductionSteps),
                CertificatesTitle = content.CertificatesTitle,
                CertificatesSubtitle = content.CertificatesSubtitle,
                CertificateItems = DeserializeCertificates(content.CertificateItems),
                RegionTitle = content.RegionTitle,
                RegionContent = content.RegionContent,
                RegionImageUrl = content.RegionImageUrl,
                RegionFeatures = DeserializeRegionFeatures(content.RegionFeatures),
                CtaTitle = content.CtaTitle,
                CtaContent = content.CtaContent,
                CtaButtonText = content.CtaButtonText,
                CtaSecondButtonText = content.CtaSecondButtonText,
                StoryFeatures = DeserializeStoryFeatures(content.StoryFeatures),
                IsActive = content.IsActive,
                CreatedAt = content.CreatedAt,
                UpdatedAt = content.UpdatedAt
            };
        }

        private AboutContent MapFromRequest(AboutEditRequest request)
        {
            return new AboutContent
            {
                Title = request.Title,
                Subtitle = request.Subtitle,
                StoryTitle = request.StoryTitle,
                StoryContent = request.StoryContent,
                StoryImageUrl = request.StoryImageUrl,
                MissionTitle = request.MissionTitle,
                MissionContent = request.MissionContent,
                VisionTitle = request.VisionTitle,
                VisionContent = request.VisionContent,
                ValuesTitle = request.ValuesTitle,
                ValuesSubtitle = request.ValuesSubtitle,
                ValueItems = JsonSerializer.Serialize(request.ValueItems),
                ProductionTitle = request.ProductionTitle,
                ProductionSubtitle = request.ProductionSubtitle,
                ProductionSteps = JsonSerializer.Serialize(request.ProductionSteps),
                CertificatesTitle = request.CertificatesTitle,
                CertificatesSubtitle = request.CertificatesSubtitle,
                CertificateItems = JsonSerializer.Serialize(request.CertificateItems),
                RegionTitle = request.RegionTitle,
                RegionContent = request.RegionContent,
                RegionImageUrl = request.RegionImageUrl,
                RegionFeatures = JsonSerializer.Serialize(request.RegionFeatures),
                CtaTitle = request.CtaTitle,
                CtaContent = request.CtaContent,
                CtaButtonText = request.CtaButtonText,
                CtaSecondButtonText = request.CtaSecondButtonText,
                StoryFeatures = JsonSerializer.Serialize(request.StoryFeatures),
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        private void UpdateExistingContent(AboutContent existing, AboutEditRequest request)
        {
            existing.Title = request.Title;
            existing.Subtitle = request.Subtitle;
            existing.StoryTitle = request.StoryTitle;
            existing.StoryContent = request.StoryContent;
            existing.StoryImageUrl = request.StoryImageUrl;
            existing.MissionTitle = request.MissionTitle;
            existing.MissionContent = request.MissionContent;
            existing.VisionTitle = request.VisionTitle;
            existing.VisionContent = request.VisionContent;
            existing.ValuesTitle = request.ValuesTitle;
            existing.ValuesSubtitle = request.ValuesSubtitle;
            existing.ValueItems = JsonSerializer.Serialize(request.ValueItems);
            existing.ProductionTitle = request.ProductionTitle;
            existing.ProductionSubtitle = request.ProductionSubtitle;
            existing.ProductionSteps = JsonSerializer.Serialize(request.ProductionSteps);
            existing.CertificatesTitle = request.CertificatesTitle;
            existing.CertificatesSubtitle = request.CertificatesSubtitle;
            existing.CertificateItems = JsonSerializer.Serialize(request.CertificateItems);
            existing.RegionTitle = request.RegionTitle;
            existing.RegionContent = request.RegionContent;
            existing.RegionImageUrl = request.RegionImageUrl;
            existing.RegionFeatures = JsonSerializer.Serialize(request.RegionFeatures);
            existing.CtaTitle = request.CtaTitle;
            existing.CtaContent = request.CtaContent;
            existing.CtaButtonText = request.CtaButtonText;
            existing.CtaSecondButtonText = request.CtaSecondButtonText;
            existing.StoryFeatures = JsonSerializer.Serialize(request.StoryFeatures);
            existing.UpdatedAt = DateTime.Now;
        }

        // JSON Deserialization helpers
        private List<ValueItemRequest> DeserializeValueItems(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultValueItems();
            try
            {
                return JsonSerializer.Deserialize<List<ValueItemRequest>>(json) ?? GetDefaultValueItems();
            }
            catch
            {
                return GetDefaultValueItems();
            }
        }

        private List<ProductionStepRequest> DeserializeProductionSteps(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultProductionSteps();
            try
            {
                return JsonSerializer.Deserialize<List<ProductionStepRequest>>(json) ?? GetDefaultProductionSteps();
            }
            catch
            {
                return GetDefaultProductionSteps();
            }
        }

        private List<CertificateItemRequest> DeserializeCertificates(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultCertificates();
            try
            {
                return JsonSerializer.Deserialize<List<CertificateItemRequest>>(json) ?? GetDefaultCertificates();
            }
            catch
            {
                return GetDefaultCertificates();
            }
        }

        private List<RegionFeatureRequest> DeserializeRegionFeatures(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultRegionFeatures();
            try
            {
                return JsonSerializer.Deserialize<List<RegionFeatureRequest>>(json) ?? GetDefaultRegionFeatures();
            }
            catch
            {
                return GetDefaultRegionFeatures();
            }
        }

        private List<StoryFeatureRequest> DeserializeStoryFeatures(string? json)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultStoryFeatures();
            try
            {
                return JsonSerializer.Deserialize<List<StoryFeatureRequest>>(json) ?? GetDefaultStoryFeatures();
            }
            catch
            {
                return GetDefaultStoryFeatures();
            }
        }

        // Default data providers
        private List<ValueItemRequest> GetDefaultValueItems()
        {
            return new List<ValueItemRequest>
            {
                new() { Title = "Doğallık", Content = "Hiçbir katkı maddesi kullanmadan, tamamen doğal yöntemlerle üretim yapıyoruz.", Icon = "fas fa-leaf", Color = "success" },
                new() { Title = "Kalite", Content = "En yüksek kalite standartlarında üretim yaparak müşteri memnuniyetini sağlıyoruz.", Icon = "fas fa-award", Color = "warning" },
                new() { Title = "Gelenek", Content = "Nesillerden gelen geleneksel üretim yöntemlerini koruyor ve yaşatıyoruz.", Icon = "fas fa-history", Color = "primary" },
                new() { Title = "Güven", Content = "Müşterilerimizin güvenini kazanmak ve sürdürmek en önemli önceliğimizdir.", Icon = "fas fa-heart", Color = "danger" }
            };
        }

        private List<ProductionStepRequest> GetDefaultProductionSteps()
        {
            return new List<ProductionStepRequest>
            {
                new() { StepNumber = 1, Title = "Süt Toplama", Content = "Balıkesir'in temiz havasında yetişen ineklerden günlük olarak taze süt toplanır." },
                new() { StepNumber = 2, Title = "Kalite Kontrol", Content = "Toplanan sütler laboratuvar ortamında detaylı kalite kontrolünden geçirilir." },
                new() { StepNumber = 3, Title = "Üretim", Content = "Geleneksel yöntemlerle modern teknoloji birleştirilerek üretim yapılır." },
                new() { StepNumber = 4, Title = "Paketleme", Content = "Hijyenik koşullarda paketlenen ürünler soğuk zincirle teslim edilir." }
            };
        }

        private List<CertificateItemRequest> GetDefaultCertificates()
        {
            return new List<CertificateItemRequest>
            {
                new() { Title = "ISO 22000", Description = "Gıda Güvenliği Yönetim Sistemi", Icon = "fas fa-certificate", Color = "primary" },
                new() { Title = "HACCP", Description = "Tehlike Analizi ve Kritik Kontrol Noktaları", Icon = "fas fa-award", Color = "warning" },
                new() { Title = "Helal Sertifikası", Description = "Helal Gıda Üretim Sertifikası", Icon = "fas fa-shield-alt", Color = "success" }
            };
        }

        private List<RegionFeatureRequest> GetDefaultRegionFeatures()
        {
            return new List<RegionFeatureRequest>
            {
                new() { Title = "Balıkesir, Bandırma", Icon = "fas fa-map-marker-alt" },
                new() { Title = "Mikro Klima", Icon = "fas fa-temperature-high" },
                new() { Title = "Kaliteli Otlaklar", Icon = "fas fa-leaf" },
                new() { Title = "Temiz Su Kaynakları", Icon = "fas fa-water" }
            };
        }

        private List<StoryFeatureRequest> GetDefaultStoryFeatures()
        {
            return new List<StoryFeatureRequest>
            {
                new() { Title = "38 Yıllık Deneyim" },
                new() { Title = "Kaliteli Üretim" },
                new() { Title = "Geleneksel Yöntem" },
                new() { Title = "Kalite Garantisi" }
            };
        }
    }
}
