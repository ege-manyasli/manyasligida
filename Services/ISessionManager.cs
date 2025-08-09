using manyasligida.Models;

namespace manyasligida.Services
{
    public interface ISessionManager
    {
        // User session management
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsUserLoggedInAsync();
        Task<bool> IsUserAdminAsync();
        Task<bool> IsCurrentUserAdminAsync();
        Task<string?> GetCurrentSessionIdAsync();
        
        // Session validation
        Task<bool> IsSessionValidAsync();
        
        // Session creation and management
        Task<bool> CreateUserSessionAsync(User user);
        Task<bool> ValidateSessionAsync();
        Task<bool> ExtendSessionAsync();
        Task<bool> InvalidateSessionAsync();
        
        // Session expiry and refresh
        Task<bool> IsSessionExpiredAsync();
        Task<DateTime?> GetSessionExpiryAsync();
        Task<bool> RefreshSessionAsync();
        
        // Multi-user support
        Task<bool> IsSessionUniqueAsync(string sessionId);
        Task<bool> ForceLogoutOtherSessionsAsync(int userId);
        Task<int> GetActiveSessionCountAsync(int userId);
        
        // Session cleanup
        Task CleanupExpiredSessionsAsync();
    }
}
