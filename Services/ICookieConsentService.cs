using manyasligida.Models;

namespace manyasligida.Services
{
    public interface ICookieConsentService
    {
        Task<bool> HasUserConsentedAsync(string sessionId, int? userId = null);
        Task<CookieConsent?> GetUserConsentAsync(string sessionId, int? userId = null);
        Task<bool> SaveConsentAsync(string sessionId, int? userId, string ipAddress, string userAgent, bool isAccepted, Dictionary<int, bool> categoryPreferences);
        Task<List<CookieCategory>> GetActiveCategoriesAsync();
        Task<Dictionary<int, bool>> GetUserPreferencesAsync(string sessionId, int? userId = null);
        Task<bool> UpdatePreferencesAsync(string sessionId, int? userId, Dictionary<int, bool> categoryPreferences);
        Task<object> GetConsentStatisticsAsync();
        Task<bool> UpdateCategoryAsync(int id, CookieCategoryUpdateRequest request);
        Task<object> GetConsentAnalyticsAsync();
    }
}
