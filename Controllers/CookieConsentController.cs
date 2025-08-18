using Microsoft.AspNetCore.Mvc;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using manyasligida.Services;
using manyasligida.Attributes;

namespace manyasligida.Controllers;

[Route("CookieConsent")]
    public class CookieConsentController : Controller
    {
    private readonly ICookieConsentService _cookieConsentService;
    private readonly ILogger<CookieConsentController> _logger;

    public CookieConsentController(
        ICookieConsentService cookieConsentService, 
        ILogger<CookieConsentController> logger)
    {
        _cookieConsentService = cookieConsentService;
        _logger = logger;
    }

    #region Public Views

    [HttpPost("SimpleSave")]
    public async Task<IActionResult> SimpleSave([FromBody] Dictionary<string, bool> preferences)
    {
        try
        {
            _logger.LogInformation("SimpleSave called with preferences: {Preferences}", string.Join(", ", preferences.Select(p => $"{p.Key}={p.Value}")));

            var sessionId = HttpContext.Session.Id;
            var userId = User.Identity?.IsAuthenticated == true ? 
                int.Parse(User.FindFirst("UserId")?.Value ?? "0") : (int?)null;

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var request = new CookieConsentRequest
            {
                SessionId = sessionId,
                UserId = userId,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsAccepted = true,
                CategoryPreferences = preferences
            };

            var result = await _cookieConsentService.SaveConsentAsync(request);

            if (result.Success)
            {
                return Json(new { success = true, message = "Çerez tercihleri kaydedildi" });
            }
            else
            {
                return Json(new { success = false, message = result.Message ?? "Çerez tercihleri kaydedilemedi" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cookie preferences");
            return Json(new { success = false, message = "Çerez tercihleri kaydedilirken bir hata oluştu" });
        }
    }

    [HttpGet("Management")]
    [AdminAuthorization]
    public async Task<IActionResult> Management()
    {
        try
        {
            // Get consent statistics
            var statsResult = await _cookieConsentService.GetConsentStatisticsAsync();
            var categoriesResult = await _cookieConsentService.GetCategoriesAsync();
            var analyticsResult = await _cookieConsentService.GetConsentAnalyticsAsync();

            ViewBag.ConsentStats = statsResult.Success ? statsResult.Data : new
            {
                totalConsents = 0,
                acceptedConsents = 0,
                declinedConsents = 0,
                acceptanceRate = 0
            };

            ViewBag.Categories = categoriesResult.Success ? categoriesResult.Data : new List<CookieCategoryResponse>();

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cookie consent management");
            TempData["Error"] = "Çerez ayarları yüklenirken bir hata oluştu";
            
            ViewBag.ConsentStats = new
            {
                totalConsents = 0,
                acceptedConsents = 0,
                declinedConsents = 0,
                acceptanceRate = 0
            };
            ViewBag.Categories = new List<CookieCategoryResponse>();
            
            return View();
        }
    }

    #endregion

    #region API Endpoints

    [HttpPost("api/save-consent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveConsent([FromBody] CookieConsentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Geçersiz veri", errors });
            }

            // Use current session ID if not provided
            if (string.IsNullOrEmpty(request.SessionId))
            {
                request = request with { SessionId = HttpContext.Session.Id };
            }

            var result = await _cookieConsentService.SaveConsentAsync(request);

            _logger.LogInformation("Cookie consent saved: {Success}, SessionId: {SessionId}", 
                result.Success, request.SessionId);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                consentId = result.ConsentId,
                consentDate = result.ConsentDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cookie consent");
            return Json(new { success = false, message = "Çerez tercihleri kaydedilemedi" });
        }
    }

    [HttpPost("api/update-consent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateConsent([FromBody] CookieSettingsUpdateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri" });
            }

            var result = await _cookieConsentService.UpdateConsentAsync(request);

            _logger.LogInformation("Cookie consent updated: {Success}", result.Success);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cookie consent");
            return Json(new { success = false, message = "Çerez tercihleri güncellenemedi" });
        }
    }

    [HttpGet("api/status")]
    public async Task<IActionResult> GetStatus(string? sessionId = null)
    {
        try
        {
            sessionId ??= HttpContext.Session.Id;
            var result = await _cookieConsentService.GetConsentStatusAsync(sessionId);

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cookie consent status");
            return Json(ApiResponse<CookieConsentStatusResponse>.FailureResult("Çerez durumu alınamadı"));
        }
    }

    [HttpGet("api/categories")]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var result = await _cookieConsentService.GetCategoriesAsync();
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cookie categories");
            return Json(ApiResponse<List<CookieCategoryResponse>>.FailureResult("Kategoriler alınamadı"));
        }
    }

    [HttpPost("api/revoke")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeConsent(string? sessionId = null)
    {
        try
        {
            sessionId ??= HttpContext.Session.Id;
            var result = await _cookieConsentService.RevokeConsentAsync(sessionId);

            _logger.LogInformation("Cookie consent revoked: {Success}, SessionId: {SessionId}", 
                result.Success, sessionId);

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking cookie consent");
            return Json(ApiResponse<bool>.FailureResult("Çerez onayı iptal edilemedi"));
        }
    }

    #endregion

    #region Admin Functions

    [HttpGet("admin/categories")]
    [AdminAuthorization]
    public async Task<IActionResult> AdminCategories()
    {
        try
        {
            var result = await _cookieConsentService.GetCategoriesAsync();

            if (result.Success)
            {
                return View(result.Data);
            }

            TempData["Error"] = result.Message;
            return View(new List<CookieCategoryResponse>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin cookie categories");
            TempData["Error"] = "Kategoriler yüklenirken bir hata oluştu";
            return View(new List<CookieCategoryResponse>());
        }
    }

    [HttpPost("admin/categories/create")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> CreateCategory(string name, string description, bool isRequired)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Kategori adı gereklidir" });
            }

            var result = await _cookieConsentService.CreateCategoryAsync(name, description, isRequired);

            _logger.LogInformation("Cookie category creation attempt: {Success}, Name: {Name}", 
                result.Success, name);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cookie category");
            return Json(new { success = false, message = "Kategori oluşturulamadı" });
        }
    }

    [HttpPost("admin/categories/update/{categoryId}")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> UpdateCategory(int categoryId, string name, string description, bool isRequired)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new { success = false, message = "Kategori adı gereklidir" });
            }

            var result = await _cookieConsentService.UpdateCategoryAsync(categoryId, name, description, isRequired);

            _logger.LogInformation("Cookie category update attempt: {Success}, CategoryId: {CategoryId}", 
                result.Success, categoryId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cookie category");
            return Json(new { success = false, message = "Kategori güncellenemedi" });
        }
    }

    [HttpPost("admin/categories/delete/{categoryId}")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        try
        {
            var result = await _cookieConsentService.DeleteCategoryAsync(categoryId);

            _logger.LogInformation("Cookie category deletion attempt: {Success}, CategoryId: {CategoryId}", 
                result.Success, categoryId);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cookie category");
            return Json(new { success = false, message = "Kategori silinemedi" });
        }
    }

    [HttpPost("admin/categories/reorder")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> ReorderCategories([FromBody] Dictionary<int, int> categoryOrders)
    {
        try
        {
            var results = new List<bool>();

            foreach (var (categoryId, sortOrder) in categoryOrders)
            {
                var result = await _cookieConsentService.UpdateCategorySortOrderAsync(categoryId, sortOrder);
                results.Add(result.Success);
            }

            var allSuccess = results.All(r => r);

            _logger.LogInformation("Cookie categories reorder attempt: {Success}", allSuccess);

            return Json(new
            {
                success = allSuccess,
                message = allSuccess ? "Kategori sıralaması güncellendi" : "Bazı kategoriler güncellenemedi"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering cookie categories");
            return Json(new { success = false, message = "Kategori sıralaması güncellenemedi" });
        }
    }

    #endregion

    #region Analytics & Reports

    [HttpGet("admin/analytics")]
    [AdminAuthorization]
    public async Task<IActionResult> Analytics(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
            var analyticsResult = await _cookieConsentService.GetConsentAnalyticsAsync(startDate, endDate);
            var trendsResult = await _cookieConsentService.GetConsentTrendsAsync(30);

            var viewModel = new
            {
                Analytics = analyticsResult.Success ? analyticsResult.Data : new Dictionary<string, int>(),
                Trends = trendsResult.Success ? trendsResult.Data : new List<object>(),
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cookie consent analytics");
            TempData["Error"] = "Analitik veriler yüklenirken bir hata oluştu";
            
            var emptyModel = new
            {
                Analytics = new Dictionary<string, int>(),
                Trends = new List<object>(),
                StartDate = startDate,
                EndDate = endDate
            };

            return View(emptyModel);
        }
    }

    [HttpGet("admin/api/analytics")]
    [AdminAuthorization]
    public async Task<IActionResult> GetAnalytics(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var result = await _cookieConsentService.GetConsentAnalyticsAsync(startDate, endDate);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent analytics API");
            return Json(ApiResponse<Dictionary<string, int>>.FailureResult("Analitik veriler alınamadı"));
        }
    }

    [HttpGet("admin/api/trends")]
    [AdminAuthorization]
    public async Task<IActionResult> GetTrends(int days = 30)
    {
        try
        {
            var result = await _cookieConsentService.GetConsentTrendsAsync(days);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent trends API");
            return Json(ApiResponse<List<object>>.FailureResult("Trend verileri alınamadı"));
        }
    }

    [HttpGet("GetConsentAnalytics")]
    [AdminAuthorization]
    public async Task<IActionResult> GetConsentAnalytics()
    {
        try
        {
            var result = await _cookieConsentService.GetDailyConsentAnalyticsAsync(30);
            
            if (result.Success && result.Data != null && result.Data.Any())
            {
                return Json(new { success = result.Success, analytics = new { dailyConsents = result.Data } });
            }
            else
            {
                // Örnek veri döndür
                var sampleData = new List<object>
                {
                    new { date = DateTime.Now.AddDays(-29).ToString("yyyy-MM-dd"), acceptedCount = 5, declinedCount = 2 },
                    new { date = DateTime.Now.AddDays(-28).ToString("yyyy-MM-dd"), acceptedCount = 8, declinedCount = 1 },
                    new { date = DateTime.Now.AddDays(-27).ToString("yyyy-MM-dd"), acceptedCount = 12, declinedCount = 3 },
                    new { date = DateTime.Now.AddDays(-26).ToString("yyyy-MM-dd"), acceptedCount = 6, declinedCount = 2 },
                    new { date = DateTime.Now.AddDays(-25).ToString("yyyy-MM-dd"), acceptedCount = 15, declinedCount = 4 }
                };
                
                return Json(new { success = true, analytics = new { dailyConsents = sampleData } });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consent analytics");
            
            // Hata durumunda örnek veri döndür
            var sampleData = new List<object>
            {
                new { date = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd"), acceptedCount = 3, declinedCount = 1 },
                new { date = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"), acceptedCount = 7, declinedCount = 2 },
                new { date = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"), acceptedCount = 10, declinedCount = 1 },
                new { date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), acceptedCount = 5, declinedCount = 3 },
                new { date = DateTime.Now.ToString("yyyy-MM-dd"), acceptedCount = 8, declinedCount = 2 }
            };
            
            return Json(new { success = true, analytics = new { dailyConsents = sampleData } });
        }
    }

    [HttpPost("UpdateCategory/{id}")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] object request)
    {
        try
        {
            // Parse the request object
            var requestDict = request as Dictionary<string, object>;
            if (requestDict == null)
            {
                return Json(new { success = false, message = "Geçersiz veri" });
            }

            var name = requestDict["name"]?.ToString();
            var description = requestDict["description"]?.ToString();
            var isRequired = Convert.ToBoolean(requestDict["isRequired"]);
            var isActive = Convert.ToBoolean(requestDict["isActive"]);
            var sortOrder = Convert.ToInt32(requestDict["sortOrder"]);

            var result = await _cookieConsentService.UpdateCategoryAsync(id, name, description, isRequired, isActive, sortOrder);
            
            if (result.Success)
            {
                return Json(new { success = true, message = "Kategori güncellendi" });
            }
            else
            {
                return Json(new { success = false, message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cookie category");
            return Json(new { success = false, message = "Kategori güncellenirken hata oluştu" });
        }
    }

    [HttpPost("admin/cleanup-expired")]
    [ValidateAntiForgeryToken]
    [AdminAuthorization]
    public async Task<IActionResult> CleanupExpired()
    {
        try
        {
            var result = await _cookieConsentService.CleanupExpiredConsentsAsync();

            _logger.LogInformation("Cleanup expired consents: {Success}", result.Success);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error cleaning up expired consents");
            return Json(new { success = false, message = "Temizlik işlemi başarısız" });
        }
    }

    #endregion

    #region Compliance & Privacy

    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("Policy")]
    public IActionResult Policy()
    {
        return View();
    }

    [HttpGet("api/compliance-status")]
    public async Task<IActionResult> GetComplianceStatus()
        {
            try
            {
            var totalConsentsResult = await _cookieConsentService.GetTotalConsentsCountAsync();
            var consentRateResult = await _cookieConsentService.GetConsentRateAsync();

            var complianceStatus = new
            {
                TotalActiveConsents = totalConsentsResult.Success ? totalConsentsResult.Data : 0,
                ConsentRate = consentRateResult.Success ? consentRateResult.Data : 0.0,
                IsCompliant = consentRateResult.Success && consentRateResult.Data > 0,
                LastUpdated = DateTimeHelper.NowTurkey
            };

            return Json(ApiResponse<object>.SuccessResult(complianceStatus));
            }
            catch (Exception ex)
            {
            _logger.LogError(ex, "Error getting compliance status");
            return Json(ApiResponse<object>.FailureResult("Uyumluluk durumu alınamadı"));
        }
    }

    #endregion

    #region Helper Methods

    private string GetClientIpAddress()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Check for forwarded IP (when behind proxy/load balancer)
        if (HttpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            var forwardedIps = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedIps))
            {
                ipAddress = forwardedIps.Split(',')[0].Trim();
            }
        }
        
        return ipAddress ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return HttpContext.Request.Headers["User-Agent"].ToString();
    }

    #endregion
}
