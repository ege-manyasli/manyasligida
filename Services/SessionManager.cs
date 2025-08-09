using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;

namespace manyasligida.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionManager> _logger;

        public SessionManager(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<SessionManager> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                {
                    // Fallback to claims-based auth cookie
                    var principal = _httpContextAccessor.HttpContext?.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(userIdClaim, out var userId))
                        {
                            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                            if (userFromDb != null)
                            {
                                // Recreate app session for this authenticated user
                                await CreateUserSessionAsync(userFromDb);
                                return userFromDb;
                            }
                        }
                    }
                    return null;
                }

                // Get active session from database
                var userSession = await _context.UserSessions
                    .Include(us => us.User)
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId && us.IsActive && us.ExpiresAt > DateTime.UtcNow);

                if (userSession?.User == null)
                    return null;

                // Update last activity
                userSession.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return userSession.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            var user = await GetCurrentUserAsync();
            return user != null;
        }

        public async Task<bool> IsUserAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.IsAdmin == true;
        }

        public async Task<bool> IsCurrentUserAdminAsync()
        {
            return await IsUserAdminAsync();
        }

        public async Task<string?> GetCurrentSessionIdAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            return session.GetString("SessionId");
        }

        public async Task<bool> CreateUserSessionAsync(User user)
        {
            try
            {
                // Generate unique session ID
                var sessionId = GenerateUniqueSessionId();
                
                // Get client information
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
                var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();
                var deviceInfo = GetDeviceInfo(httpContext?.Request.Headers["User-Agent"].ToString());

                // Create session expiry (2 hours from now)
                var expiresAt = DateTime.UtcNow.AddHours(2);

                // Create new user session
                var userSession = new UserSession
                {
                    SessionId = sessionId,
                    UserId = user.Id,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceInfo = deviceInfo,
                    CreatedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsActive = true
                };

                _context.UserSessions.Add(userSession);
                await _context.SaveChangesAsync();

                // Set session in HTTP context
                var session = httpContext?.Session;
                if (session != null)
                {
                    // Session'ı temizlemeden önce kontrol et
                    try
                    {
                        session.SetString("SessionId", sessionId);
                        session.SetString("UserId", user.Id.ToString());
                        session.SetString("UserName", user.FullName);
                        session.SetString("UserEmail", user.Email);
                        session.SetString("IsAdmin", user.IsAdmin.ToString());
                        session.SetString("LoginTime", DateTime.UtcNow.ToString("O"));
                    }
                    catch (Exception sessionEx)
                    {
                        _logger.LogError(sessionEx, "Error setting session data for user {UserId}", user.Id);
                        // Session hatası olsa bile devam et
                    }
                }
                else
                {
                    _logger.LogWarning("Session is null for user {UserId}", user.Id);
                }

                _logger.LogInformation("Session created for user {UserId} with session {SessionId}", user.Id, sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user session for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> ValidateSessionAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                {
                    // If user has auth cookie, recreate session and accept as valid
                    var principal = _httpContextAccessor.HttpContext?.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(userIdClaim, out var userId))
                        {
                            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                            if (userFromDb != null)
                            {
                                await CreateUserSessionAsync(userFromDb);
                                return true;
                            }
                        }
                    }
                    return false;
                }

                var userSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId && us.IsActive);

                if (userSession == null || userSession.ExpiresAt <= DateTime.UtcNow)
                {
                    await InvalidateSessionAsync();
                    return false;
                }

                // Update last activity
                userSession.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating session");
                return false;
            }
        }

        public async Task<bool> ExtendSessionAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                {
                    // Try to recreate if auth cookie exists
                    var principal = _httpContextAccessor.HttpContext?.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(userIdClaim, out var userId))
                        {
                            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                            if (userFromDb != null)
                            {
                                await CreateUserSessionAsync(userFromDb);
                                return true;
                            }
                        }
                    }
                    return false;
                }

                var userSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId && us.IsActive);

                if (userSession == null)
                    return false;

                // Extend session by 2 hours
                userSession.ExpiresAt = DateTime.UtcNow.AddHours(2);
                userSession.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending session");
                return false;
            }
        }

        public async Task<bool> InvalidateSessionAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (!string.IsNullOrEmpty(sessionId))
                {
                    // Mark session as inactive in database
                    var userSession = await _context.UserSessions
                        .FirstOrDefaultAsync(us => us.SessionId == sessionId);
                    
                    if (userSession != null)
                    {
                        userSession.IsActive = false;
                        await _context.SaveChangesAsync();
                    }
                }

                // Clear HTTP session
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session != null)
                {
                    session.Clear();
                }

                // Clear cookies
                var response = _httpContextAccessor.HttpContext?.Response;
                if (response != null)
                {
                    response.Cookies.Delete(".ManyasliGida.Session");
                    response.Cookies.Delete(".AspNetCore.Session");
                    response.Cookies.Delete("ASP.NET_SessionId");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating session");
                return false;
            }
        }

        public async Task<bool> IsSessionValidAsync()
        {
            return await ValidateSessionAsync();
        }

        public async Task<bool> IsSessionUniqueAsync(string sessionId)
        {
            try
            {
                var existingSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId && us.IsActive);

                return existingSession == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session uniqueness");
                return false;
            }
        }

        public async Task<bool> IsSessionExpiredAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                    return true;

                var userSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId);

                return userSession?.ExpiresAt <= DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session expiry");
                return true;
            }
        }

        public async Task<DateTime?> GetSessionExpiryAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                    return null;

                var userSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId);

                return userSession?.ExpiresAt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session expiry");
                return null;
            }
        }

        public async Task<bool> RefreshSessionAsync()
        {
            try
            {
                var sessionId = await GetCurrentSessionIdAsync();
                if (string.IsNullOrEmpty(sessionId))
                {
                    // Try to recreate if auth cookie exists
                    var principal = _httpContextAccessor.HttpContext?.User;
                    if (principal?.Identity?.IsAuthenticated == true)
                    {
                        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        if (int.TryParse(userIdClaim, out var userId))
                        {
                            var userFromDb = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                            if (userFromDb != null)
                            {
                                await CreateUserSessionAsync(userFromDb);
                                return true;
                            }
                        }
                    }
                    return false;
                }

                var userSession = await _context.UserSessions
                    .FirstOrDefaultAsync(us => us.SessionId == sessionId && us.IsActive);

                if (userSession == null)
                    return false;

                // Refresh session expiry
                userSession.ExpiresAt = DateTime.UtcNow.AddHours(2);
                userSession.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing session");
                return false;
            }
        }

        public async Task<bool> ForceLogoutOtherSessionsAsync(int userId)
        {
            try
            {
                var currentSessionId = await GetCurrentSessionIdAsync();
                
                var otherSessions = await _context.UserSessions
                    .Where(us => us.UserId == userId && us.IsActive && us.SessionId != currentSessionId)
                    .ToListAsync();

                foreach (var session in otherSessions)
                {
                    session.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error forcing logout other sessions for user {UserId}", userId);
                return false;
            }
        }



        public async Task<int> GetActiveSessionCountAsync(int userId)
        {
            try
            {
                return await _context.UserSessions
                    .CountAsync(us => us.UserId == userId && us.IsActive && us.ExpiresAt > DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active session count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            try
            {
                var expiredSessions = await _context.UserSessions
                    .Where(us => us.ExpiresAt <= DateTime.UtcNow && us.IsActive)
                    .ToListAsync();

                foreach (var session in expiredSessions)
                {
                    session.IsActive = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired sessions");
            }
        }

        private string GenerateUniqueSessionId()
        {
            var sessionId = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 22);

            return sessionId;
        }

        private string GetDeviceInfo(string? userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            if (userAgent.Contains("Mobile"))
                return "Mobile";
            if (userAgent.Contains("Tablet"))
                return "Tablet";
            if (userAgent.Contains("Windows"))
                return "Windows";
            if (userAgent.Contains("Mac"))
                return "Mac";
            if (userAgent.Contains("Linux"))
                return "Linux";

            return "Desktop";
        }
    }
}
