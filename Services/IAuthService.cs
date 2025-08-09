using manyasligida.Models;

namespace manyasligida.Services
{
    public interface IAuthService
    {
        // Authentication methods
        Task<User?> LoginAsync(string email, string password);
        Task<User?> RegisterAsync(RegisterViewModel model);
        Task<bool> IsEmailExistsAsync(string email);
        Task Logout();

        // Password methods
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);

        // User management methods
        void SetCurrentUser(User user);
        Task<bool> VerifyEmailAsync(string email, string verificationCode);
        Task<bool> ResendVerificationCodeAsync(string email);

        // Password management
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // Session management methods
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsCurrentUserAdminAsync();
        Task<bool> IsUserLoggedInAsync();
        Task<bool> ValidateSessionAsync();
        Task<bool> ExtendSessionAsync();
        Task<bool> RefreshSessionAsync();
        Task<bool> IsSessionExpiredAsync();
        Task<DateTime?> GetSessionExpiryAsync();
        Task CleanupExpiredSessionsAsync();

        // Multi-session management
        Task<bool> ForceLogoutOtherSessionsAsync();
        Task<int> GetActiveSessionCountAsync();
    }
}
