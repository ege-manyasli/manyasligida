using manyasligida.Models;
using manyasligida.Data;
using Microsoft.EntityFrameworkCore;

namespace manyasligida.Services
{
    public interface ISiteSettingsService
    {
        Task<SiteSettings> GetAsync();
        Task<bool> UpdateAsync(SiteSettings settings);
        Task<bool> InitializeDefaultSettingsAsync();
    }

    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SiteSettingsService> _logger;

        public SiteSettingsService(ApplicationDbContext context, ILogger<SiteSettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SiteSettings> GetAsync()
        {
            try
            {
                var settings = await _context.SiteSettings
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (settings == null)
                {
                    // Varsayılan ayarları oluştur
                    await InitializeDefaultSettingsAsync();
                    settings = await _context.SiteSettings
                        .Where(s => s.IsActive)
                        .FirstOrDefaultAsync();
                }

                return settings ?? new SiteSettings();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting site settings");
                return new SiteSettings();
            }
        }

        public async Task<bool> UpdateAsync(SiteSettings settings)
        {
            try
            {
                var existingSettings = await _context.SiteSettings
                    .Where(s => s.IsActive)
                    .FirstOrDefaultAsync();

                if (existingSettings != null)
                {
                    // Mevcut ayarları güncelle
                    existingSettings.Phone = settings.Phone;
                    existingSettings.Email = settings.Email;
                    existingSettings.Address = settings.Address;
                    existingSettings.WorkingHours = settings.WorkingHours;
                    existingSettings.FacebookUrl = settings.FacebookUrl;
                    existingSettings.InstagramUrl = settings.InstagramUrl;
                    existingSettings.TwitterUrl = settings.TwitterUrl;
                    existingSettings.YoutubeUrl = settings.YoutubeUrl;
                    existingSettings.SiteTitle = settings.SiteTitle;
                    existingSettings.SiteDescription = settings.SiteDescription;
                    existingSettings.SiteKeywords = settings.SiteKeywords;
                    existingSettings.LogoUrl = settings.LogoUrl;
                    existingSettings.FaviconUrl = settings.FaviconUrl;
                    existingSettings.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Yeni ayarlar oluştur
                    settings.CreatedAt = DateTime.Now;
                    settings.IsActive = true;
                    _context.SiteSettings.Add(settings);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Site settings updated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating site settings");
                return false;
            }
        }

        public async Task<bool> InitializeDefaultSettingsAsync()
        {
            try
            {
                var defaultSettings = new SiteSettings
                {
                    Phone = "+90 266 123 45 67",
                    Email = "info@manyasligida.com",
                    Address = "17 Eylül, Hal Cd. No:6 D:8, 10200 Bandırma/Balıkesir",
                    WorkingHours = "Pzt-Cmt: 08:00-18:00",
                    FacebookUrl = "#",
                    InstagramUrl = "#",
                    TwitterUrl = "#",
                    YoutubeUrl = "#",
                    SiteTitle = "Manyaslı Süt Ürünleri",
                    SiteDescription = "Kaliteli ve taze süt ürünleri",
                    SiteKeywords = "süt, peynir, yoğurt, manyas, gıda",
                    LogoUrl = "/logomanyasli.png",
                    FaviconUrl = "/favicon.ico",
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.SiteSettings.Add(defaultSettings);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Default site settings initialized");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default site settings");
                return false;
            }
        }
    }
}

