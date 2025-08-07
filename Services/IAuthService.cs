using manyasligida.Models;

namespace manyasligida.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User?> RegisterAsync(RegisterViewModel model);
        Task<bool> IsEmailExistsAsync(string email);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsCurrentUserAdminAsync();
        void SetCurrentUser(User user);
        void Logout();
        Task<bool> SendVerificationCodeAsync(string email);
        Task<bool> VerifyEmailAsync(string email, string code);
        Task<bool> ResendVerificationCodeAsync(string email);
    }
}
