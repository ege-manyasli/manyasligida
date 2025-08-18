using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;

namespace manyasligida.Services;

public class CookieConsentService : ICookieConsentService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CookieConsentService> _logger;

    public CookieConsentService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CookieConsentService> logger)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<CookieConsentResponse> SaveConsentAsync(CookieConsentRequest request)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return new CookieConsentResponse
                {
                    Success = false,
                    Message = "HTTP bağlamı bulunamadı"
                };
            }

            var sessionId = request.SessionId;
            var userId = GetCurrentUserId();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            // Check for existing consent
            var existingConsent = await _context.CookieConsents
                .Include(c => c.ConsentDetails)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);

            if (existingConsent != null)
            {
                // Update existing consent
                existingConsent.ConsentDate = DateTime.UtcNow;
                existingConsent.ExpiryDate = DateTime.UtcNow.AddYears(1);
                existingConsent.UserId = userId;
                existingConsent.IpAddress = ipAddress;
                existingConsent.UserAgent = userAgent;

                // Remove old consent details
                _context.CookieConsentDetails.RemoveRange(existingConsent.ConsentDetails);
            }
            else
            {
                // Create new consent
                existingConsent = new CookieConsent
                {
                    SessionId = sessionId,
                    UserId = userId,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    ConsentDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CookieConsents.Add(existingConsent);
            }

            await _context.SaveChangesAsync();

            // Add consent details
            var consentDetails = new List<CookieConsentDetail>();

            if (request.AcceptAll)
            {
                // Accept all categories
                var allCategories = await _context.CookieCategories
                    .Where(c => c.IsActive)
                    .ToListAsync();

                foreach (var category in allCategories)
                {
                    consentDetails.Add(new CookieConsentDetail
                    {
                        CookieConsentId = existingConsent.Id,
                        CookieCategoryId = category.Id,
                        IsAccepted = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                // Accept specific categories
                foreach (var categoryConsent in request.CategoryConsents)
                {
                    consentDetails.Add(new CookieConsentDetail
                    {
                        CookieConsentId = existingConsent.Id,
                        CookieCategoryId = categoryConsent.CategoryId,
                        IsAccepted = categoryConsent.IsAccepted,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.CookieConsentDetails.AddRange(consentDetails);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cookie consent saved for session {SessionId}", sessionId);

            return new CookieConsentResponse
            {
                Success = true,
                Message = "Çerez tercihleri kaydedildi",
                ConsentId = existingConsent.Id.ToString(),
                ConsentDate = existingConsent.ConsentDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cookie consent");
            return new CookieConsentResponse
            {
                Success = false,
                Message = "Çerez tercihleri kaydedilemedi"
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateConsentAsync(CookieSettingsUpdateRequest request)
    {
        try
        {
            var sessionId = GetCurrentSessionId();
            if (string.IsNullOrEmpty(sessionId))
            {
                return ApiResponse<bool>.FailureResult("Oturum bulunamadı");
            }

            var consent = await _context.CookieConsents
                .Include(c => c.ConsentDetails)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);

            if (consent == null)
            {
                return ApiResponse<bool>.FailureResult("Mevcut çerez tercihi bulunamadı");
            }

            // Remove old consent details
            _context.CookieConsentDetails.RemoveRange(consent.ConsentDetails);

            // Add new consent details
            var consentDetails = request.CategoryConsents.Select(cc => new CookieConsentDetail
            {
                CookieConsentId = consent.Id,
                CookieCategoryId = cc.CategoryId,
                IsAccepted = cc.IsAccepted,
                CreatedAt = DateTime.UtcNow
            });

            _context.CookieConsentDetails.AddRange(consentDetails);

            // Update consent timestamp
            consent.ConsentDate = DateTime.UtcNow;
            consent.ExpiryDate = DateTime.UtcNow.AddYears(1);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cookie consent updated for session {SessionId}", sessionId);

            return ApiResponse<bool>.SuccessResult(true, "Çerez tercihleri güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cookie consent");
            return ApiResponse<bool>.FailureResult("Çerez tercihleri güncellenemedi");
        }
    }

    public async Task<ApiResponse<CookieConsentStatusResponse>> GetConsentStatusAsync(string sessionId)
    {
        try
        {
            var consent = await _context.CookieConsents
                .Include(c => c.ConsentDetails)
                .ThenInclude(cd => cd.CookieCategory)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);

            var categories = await _context.CookieCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var categoryResponses = categories.Select(category =>
            {
                var consentDetail = consent?.ConsentDetails
                    .FirstOrDefault(cd => cd.CookieCategoryId == category.Id);

                return new CookieCategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsRequired = category.IsRequired,
                    IsAccepted = consentDetail?.IsAccepted ?? category.IsRequired,
                    SortOrder = category.SortOrder
                };
            }).ToList();

            var response = new CookieConsentStatusResponse
            {
                HasConsent = consent != null,
                ConsentDate = consent?.ConsentDate,
                Categories = categoryResponses,
                NeedsUpdate = consent?.ExpiryDate < DateTime.UtcNow
            };

            return ApiResponse<CookieConsentStatusResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent status");
            return ApiResponse<CookieConsentStatusResponse>.FailureResult("Çerez durumu alınamadı");
        }
    }

    public async Task<ApiResponse<bool>> RevokeConsentAsync(string sessionId)
    {
        try
        {
            var consent = await _context.CookieConsents
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.IsActive);

            if (consent != null)
            {
                consent.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cookie consent revoked for session {SessionId}", sessionId);
            }

            return ApiResponse<bool>.SuccessResult(true, "Çerez onayı iptal edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking consent");
            return ApiResponse<bool>.FailureResult("Çerez onayı iptal edilemedi");
        }
    }

    public async Task<ApiResponse<List<CookieCategoryResponse>>> GetCategoriesAsync()
    {
        try
        {
            var categories = await _context.CookieCategories
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var categoryResponses = categories.Select(c => new CookieCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsRequired = c.IsRequired,
                IsActive = c.IsActive,
                IsAccepted = c.IsRequired, // Default to required
                SortOrder = c.SortOrder
            }).ToList();

            return ApiResponse<List<CookieCategoryResponse>>.SuccessResult(categoryResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cookie categories");
            return ApiResponse<List<CookieCategoryResponse>>.FailureResult("Çerez kategorileri alınamadı");
        }
    }

    public async Task<ApiResponse<CookieCategoryResponse>> GetCategoryByIdAsync(int categoryId)
    {
        try
        {
            var category = await _context.CookieCategories.FindAsync(categoryId);
            if (category == null)
            {
                return ApiResponse<CookieCategoryResponse>.FailureResult("Kategori bulunamadı");
            }

            var response = new CookieCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsRequired = category.IsRequired,
                IsAccepted = category.IsRequired,
                SortOrder = category.SortOrder
            };

            return ApiResponse<CookieCategoryResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cookie category");
            return ApiResponse<CookieCategoryResponse>.FailureResult("Kategori alınamadı");
        }
    }

    // Admin functions
    public async Task<ApiResponse<bool>> CreateCategoryAsync(string name, string description, bool isRequired)
    {
        try
        {
            var category = new CookieCategory
            {
                Name = name,
                Description = description,
                IsRequired = isRequired,
                IsActive = true,
                SortOrder = await GetNextSortOrderAsync(),
                CreatedAt = DateTime.UtcNow
            };

            _context.CookieCategories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cookie category created: {CategoryName}", name);

            return ApiResponse<bool>.SuccessResult(true, "Kategori oluşturuldu");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cookie category");
            return ApiResponse<bool>.FailureResult("Kategori oluşturulamadı");
        }
    }

    public async Task<ApiResponse<bool>> UpdateCategoryAsync(int categoryId, string name, string description, bool isRequired, bool isActive = true, int sortOrder = 0)
    {
        try
        {
            var category = await _context.CookieCategories.FindAsync(categoryId);
            if (category == null)
            {
                return ApiResponse<bool>.FailureResult("Kategori bulunamadı");
            }

            category.Name = name;
            category.Description = description;
            category.IsRequired = isRequired;
            category.IsActive = isActive;
            category.SortOrder = sortOrder;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cookie category updated: {CategoryId}", categoryId);

            return ApiResponse<bool>.SuccessResult(true, "Kategori güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cookie category");
            return ApiResponse<bool>.FailureResult("Kategori güncellenemedi");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var category = await _context.CookieCategories.FindAsync(categoryId);
            if (category == null)
            {
                return ApiResponse<bool>.FailureResult("Kategori bulunamadı");
            }

            if (category.IsRequired)
            {
                return ApiResponse<bool>.FailureResult("Gerekli kategori silinemez");
            }

            category.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cookie category deleted: {CategoryId}", categoryId);

            return ApiResponse<bool>.SuccessResult(true, "Kategori silindi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cookie category");
            return ApiResponse<bool>.FailureResult("Kategori silinemedi");
        }
    }

    public async Task<ApiResponse<bool>> UpdateCategorySortOrderAsync(int categoryId, int sortOrder)
    {
        try
        {
            var category = await _context.CookieCategories.FindAsync(categoryId);
            if (category == null)
            {
                return ApiResponse<bool>.FailureResult("Kategori bulunamadı");
            }

            category.SortOrder = sortOrder;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Sıralama güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sort order");
            return ApiResponse<bool>.FailureResult("Sıralama güncellenemedi");
        }
    }

    // Statistics
    public async Task<ApiResponse<object>> GetConsentStatisticsAsync()
    {
        try
        {
            var totalConsents = await _context.CookieConsents.CountAsync();
            
            // Count accepted consents based on CookieConsentDetail.IsAccepted
            var acceptedConsents = await _context.CookieConsentDetails
                .Where(cd => cd.IsAccepted)
                .Select(cd => cd.CookieConsentId)
                .Distinct()
                .CountAsync();
                
            var declinedConsents = totalConsents - acceptedConsents;
            var acceptanceRate = totalConsents > 0 ? (acceptedConsents * 100.0 / totalConsents) : 0;

            var stats = new
            {
                totalConsents,
                acceptedConsents,
                declinedConsents,
                acceptanceRate = Math.Round(acceptanceRate, 1)
            };

            return ApiResponse<object>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent statistics");
            return ApiResponse<object>.FailureResult("İstatistikler alınamadı");
        }
    }

    // Analytics
    public async Task<ApiResponse<Dictionary<string, int>>> GetConsentAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.CookieConsents.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.ConsentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.ConsentDate <= endDate.Value);

            var analytics = new Dictionary<string, int>
            {
                ["TotalConsents"] = await query.CountAsync(),
                ["ActiveConsents"] = await query.CountAsync(c => c.IsActive),
                ["ExpiredConsents"] = await query.CountAsync(c => c.ExpiryDate < DateTime.UtcNow),
                ["TodayConsents"] = await query.CountAsync(c => c.ConsentDate.Date == DateTime.UtcNow.Date)
            };

            return ApiResponse<Dictionary<string, int>>.SuccessResult(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent analytics");
            return ApiResponse<Dictionary<string, int>>.FailureResult("Analitik veriler alınamadı");
        }
    }

    // Get daily consent analytics for chart
    public async Task<ApiResponse<List<object>>> GetDailyConsentAnalyticsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            
            var dailyConsents = await _context.CookieConsents
                .Include(c => c.ConsentDetails)
                .Where(c => c.ConsentDate >= startDate)
                .GroupBy(c => c.ConsentDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    TotalCount = g.Count(),
                    AcceptedCount = g.SelectMany(c => c.ConsentDetails.Where(cd => cd.IsAccepted))
                        .Select(cd => cd.CookieConsentId)
                        .Distinct()
                        .Count(),
                    DeclinedCount = g.SelectMany(c => c.ConsentDetails.Where(cd => !cd.IsAccepted))
                        .Select(cd => cd.CookieConsentId)
                        .Distinct()
                        .Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var result = dailyConsents.Cast<object>().ToList();
            return ApiResponse<List<object>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily consent analytics");
            return ApiResponse<List<object>>.FailureResult("Günlük analitik veriler alınamadı");
        }
    }

    public async Task<ApiResponse<List<object>>> GetConsentTrendsAsync(int days = 30)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);

            var trends = await _context.CookieConsents
                .Where(c => c.ConsentDate >= startDate)
                .GroupBy(c => c.ConsentDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return ApiResponse<List<object>>.SuccessResult(trends.Cast<object>().ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent trends");
            return ApiResponse<List<object>>.FailureResult("Trend verileri alınamadı");
        }
    }

    // Compliance
    public async Task<ApiResponse<bool>> CleanupExpiredConsentsAsync()
    {
        try
        {
            var expiredConsents = await _context.CookieConsents
                .Where(c => c.ExpiryDate < DateTime.UtcNow && c.IsActive)
                .ToListAsync();

            foreach (var consent in expiredConsents)
            {
                consent.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired consents", expiredConsents.Count);

            return ApiResponse<bool>.SuccessResult(true, $"{expiredConsents.Count} süresi dolmuş onay temizlendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired consents");
            return ApiResponse<bool>.FailureResult("Süresi dolmuş onaylar temizlenemedi");
        }
    }

    public async Task<ApiResponse<int>> GetTotalConsentsCountAsync()
    {
        try
        {
            var count = await _context.CookieConsents.CountAsync(c => c.IsActive);
            return ApiResponse<int>.SuccessResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total consents count");
            return ApiResponse<int>.FailureResult("Toplam onay sayısı alınamadı");
        }
    }

    public async Task<ApiResponse<double>> GetConsentRateAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.CookieConsents.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(c => c.ConsentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.ConsentDate <= endDate.Value);

            var totalVisitors = await query.CountAsync(); // This would need session tracking
            var totalConsents = await query.CountAsync(c => c.IsActive);

            var consentRate = totalVisitors > 0 ? (double)totalConsents / totalVisitors * 100 : 0;

            return ApiResponse<double>.SuccessResult(consentRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent rate");
            return ApiResponse<double>.FailureResult("Onay oranı hesaplanamadı");
        }
    }

    // Private helper methods
    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
        }
        return null;
    }

    private string? GetCurrentSessionId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString("SessionId");
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var maxOrder = await _context.CookieCategories
            .Where(c => c.IsActive)
            .MaxAsync(c => (int?)c.SortOrder) ?? 0;

        return maxOrder + 1;
    }
}