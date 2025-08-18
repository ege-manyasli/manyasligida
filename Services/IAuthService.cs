using manyasligida.Models;
using System.Security.Claims;

namespace manyasligida.Services
{
    public interface IAuthService
    {
        // Core authentication methods
        Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false);
        Task<AuthResult> RegisterAsync(RegisterViewModel model);
        Task<bool> LogoutAsync();
        
        // User management
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsUserLoggedInAsync();
        Task<bool> IsUserAdminAsync();
        
        // Password management
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> SendPasswordResetCodeAsync(string email);
        Task<bool> VerifyPasswordResetCodeAsync(string email, string resetCode);
        
        // Email verification
        Task<bool> VerifyEmailAsync(string email, string verificationCode);
        Task<bool> ResendVerificationCodeAsync(string email);
        Task<bool> SendEmailVerificationCodeAsync(string email);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public List<Claim> Claims { get; set; } = new List<Claim>();
    }
}
