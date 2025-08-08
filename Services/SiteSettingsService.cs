using manyasligida.Models;

namespace manyasligida.Services
{
    public interface ISiteSettingsService
    {
        SiteSettings Get();
    }

    public class SiteSettingsService : ISiteSettingsService
    {
        private readonly SiteSettings _settings;
        public SiteSettingsService(IConfiguration configuration)
        {
            // İstersek appsettings.json üzerinden de besleyebiliriz; şimdilik sabit varsayılanlar
            _settings = new SiteSettings();
        }

        public SiteSettings Get() => _settings;
    }
}

