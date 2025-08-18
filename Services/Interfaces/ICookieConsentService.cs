using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces;

public interface ICookieConsentService
{
    // Consent Management
    Task<CookieConsentResponse> SaveConsentAsync(CookieConsentRequest request);
    Task<ApiResponse<bool>> UpdateConsentAsync(CookieSettingsUpdateRequest request);
    Task<ApiResponse<CookieConsentStatusResponse>> GetConsentStatusAsync(string sessionId);
    Task<ApiResponse<bool>> RevokeConsentAsync(string sessionId);
    
    // Category Management
    Task<ApiResponse<List<CookieCategoryResponse>>> GetCategoriesAsync();
    Task<ApiResponse<CookieCategoryResponse>> GetCategoryByIdAsync(int categoryId);
    
    // Admin Functions
    Task<ApiResponse<bool>> CreateCategoryAsync(string name, string description, bool isRequired);
    Task<ApiResponse<bool>> UpdateCategoryAsync(int categoryId, string name, string description, bool isRequired, bool isActive = true, int sortOrder = 0);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId);
    Task<ApiResponse<bool>> UpdateCategorySortOrderAsync(int categoryId, int sortOrder);
    
    // Statistics & Analytics
    Task<ApiResponse<object>> GetConsentStatisticsAsync();
    Task<ApiResponse<Dictionary<string, int>>> GetConsentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<ApiResponse<List<object>>> GetDailyConsentAnalyticsAsync(int days = 30);
    Task<ApiResponse<List<object>>> GetConsentTrendsAsync(int days = 30);
    
    // Compliance
    Task<ApiResponse<bool>> CleanupExpiredConsentsAsync();
    Task<ApiResponse<int>> GetTotalConsentsCountAsync();
    Task<ApiResponse<double>> GetConsentRateAsync(DateTime? startDate = null, DateTime? endDate = null);
}
