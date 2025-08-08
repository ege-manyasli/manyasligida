using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Text.Json;

namespace manyasligida.Services
{
    public class CookieConsentService : ICookieConsentService
    {
        private readonly ApplicationDbContext _context;

        public CookieConsentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasUserConsentedAsync(string sessionId, int? userId = null)
        {
            try
            {
                var query = _context.CookieConsents.AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(cc => cc.UserId == userId);
                }
                else
                {
                    query = query.Where(cc => cc.SessionId == sessionId);
                }

                return await query.AnyAsync();
            }
            catch (Exception)
            {
                // If database fails, return false to show banner
                return false;
            }
        }

        public async Task<CookieConsent?> GetUserConsentAsync(string sessionId, int? userId = null)
        {
            try
            {
                var query = _context.CookieConsents
                    .Include(cc => cc.ConsentDetails)
                    .ThenInclude(cd => cd.CookieCategory)
                    .AsQueryable();

                if (userId.HasValue)
                {
                    query = query.Where(cc => cc.UserId == userId);
                }
                else
                {
                    query = query.Where(cc => cc.SessionId == sessionId);
                }

                return await query.FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> SaveConsentAsync(string sessionId, int? userId, string ipAddress, string userAgent, bool isAccepted, Dictionary<int, bool> categoryPreferences)
        {
            try
            {
                // Check if consent already exists
                var existingConsent = await GetUserConsentAsync(sessionId, userId);
                if (existingConsent != null)
                {
                    // Update existing consent
                    existingConsent.IsAccepted = isAccepted;
                    existingConsent.ConsentDate = DateTime.Now;
                    existingConsent.IpAddress = ipAddress;
                    existingConsent.UserAgent = userAgent;
                    existingConsent.Preferences = JsonSerializer.Serialize(categoryPreferences);

                    // Update consent details
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
                        IsAccepted = isAccepted,
                        Preferences = JsonSerializer.Serialize(categoryPreferences),
                        ConsentDate = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };

                    _context.CookieConsents.Add(existingConsent);
                }

                // Add consent details
                foreach (var preference in categoryPreferences)
                {
                    var consentDetail = new CookieConsentDetail
                    {
                        CookieConsentId = existingConsent.Id,
                        CookieCategoryId = preference.Key,
                        IsAccepted = preference.Value,
                        CreatedAt = DateTime.Now
                    };

                    _context.CookieConsentDetails.Add(consentDetail);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error but don't fail
                Console.WriteLine($"SaveConsentAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CookieCategory>> GetActiveCategoriesAsync()
        {
            try
            {
                return await _context.CookieCategories
                    .Where(cc => cc.IsActive)
                    .OrderBy(cc => cc.SortOrder)
                    .ToListAsync();
            }
            catch (Exception)
            {
                // Return default categories if database fails
                return new List<CookieCategory>
                {
                    new CookieCategory { Id = 1, Name = "Gerekli Çerezler", Description = "Sitenin temel işlevselliği için gerekli olan çerezler.", IsRequired = true, IsActive = true, SortOrder = 1 },
                    new CookieCategory { Id = 2, Name = "Analitik Çerezler", Description = "Sitenin kullanımını analiz etmek için kullanılan çerezler.", IsRequired = false, IsActive = true, SortOrder = 2 },
                    new CookieCategory { Id = 3, Name = "Pazarlama Çerezleri", Description = "Kişiselleştirilmiş reklamlar için kullanılan çerezler.", IsRequired = false, IsActive = true, SortOrder = 3 },
                    new CookieCategory { Id = 4, Name = "Sosyal Medya Çerezleri", Description = "Sosyal medya platformları ile etkileşim için kullanılan çerezler.", IsRequired = false, IsActive = true, SortOrder = 4 }
                };
            }
        }

        public async Task<Dictionary<int, bool>> GetUserPreferencesAsync(string sessionId, int? userId = null)
        {
            try
            {
                var consent = await GetUserConsentAsync(sessionId, userId);
                if (consent != null && !string.IsNullOrEmpty(consent.Preferences))
                {
                    return JsonSerializer.Deserialize<Dictionary<int, bool>>(consent.Preferences) ?? new Dictionary<int, bool>();
                }
                return new Dictionary<int, bool>();
            }
            catch (Exception)
            {
                return new Dictionary<int, bool>();
            }
        }

        public async Task<bool> UpdatePreferencesAsync(string sessionId, int? userId, Dictionary<int, bool> categoryPreferences)
        {
            try
            {
                var consent = await GetUserConsentAsync(sessionId, userId);
                if (consent == null)
                    return false;

                // Update preferences
                consent.Preferences = JsonSerializer.Serialize(categoryPreferences);

                // Update consent details
                _context.CookieConsentDetails.RemoveRange(consent.ConsentDetails);

                foreach (var preference in categoryPreferences)
                {
                    var consentDetail = new CookieConsentDetail
                    {
                        CookieConsentId = consent.Id,
                        CookieCategoryId = preference.Key,
                        IsAccepted = preference.Value,
                        CreatedAt = DateTime.Now
                    };

                    _context.CookieConsentDetails.Add(consentDetail);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdatePreferencesAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<object> GetConsentStatisticsAsync()
        {
            try
            {
                var totalConsents = await _context.CookieConsents.CountAsync();
                var acceptedConsents = await _context.CookieConsents.CountAsync(cc => cc.IsAccepted);
                var declinedConsents = totalConsents - acceptedConsents;
                var acceptanceRate = totalConsents > 0 ? (double)acceptedConsents / totalConsents * 100 : 0;

                return new
                {
                    totalConsents,
                    acceptedConsents,
                    declinedConsents,
                    acceptanceRate = Math.Round(acceptanceRate, 2)
                };
            }
            catch (Exception)
            {
                return new
                {
                    totalConsents = 0,
                    acceptedConsents = 0,
                    declinedConsents = 0,
                    acceptanceRate = 0.0
                };
            }
        }

        public async Task<bool> UpdateCategoryAsync(int id, CookieCategoryUpdateRequest request)
        {
            try
            {
                var category = await _context.CookieCategories.FindAsync(id);
                if (category == null)
                    return false;

                category.Name = request.Name;
                category.Description = request.Description;
                category.IsRequired = request.IsRequired;
                category.IsActive = request.IsActive;
                category.SortOrder = request.SortOrder;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateCategoryAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<object> GetConsentAnalyticsAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var dailyConsents = await _context.CookieConsents
                    .Where(cc => cc.ConsentDate >= thirtyDaysAgo)
                    .GroupBy(cc => cc.ConsentDate.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),
                        acceptedCount = g.Count(cc => cc.IsAccepted),
                        declinedCount = g.Count(cc => !cc.IsAccepted)
                    })
                    .OrderBy(x => x.date)
                    .ToListAsync();

                return new { dailyConsents };
            }
            catch (Exception)
            {
                return new { dailyConsents = new List<object>() };
            }
        }
    }
}
