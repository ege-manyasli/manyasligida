using manyasligida.Models;
using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces;

public interface IAccountService
{
    // Authentication
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> LogoutAsync();
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    
    // Profile Management
    Task<ApiResponse<UserResponse>> GetCurrentUserAsync();
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request);
    Task<ApiResponse<bool>> DeleteAccountAsync();
    
    // Email Verification
    Task<ApiResponse<bool>> SendEmailVerificationAsync(string email);
    Task<ApiResponse<bool>> VerifyEmailAsync(EmailVerificationRequest request);
    Task<ApiResponse<bool>> ResendVerificationCodeAsync(string email);
    
    // Session Management
    Task<ApiResponse<bool>> ValidateSessionAsync();
    Task<ApiResponse<bool>> ExtendSessionAsync();
    Task<ApiResponse<int>> GetActiveSessionCountAsync();
    Task<ApiResponse<bool>> ForceLogoutOtherSessionsAsync();
    
    // Admin Functions
    Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page = 1, int pageSize = 50);
    Task<ApiResponse<UserResponse>> GetUserByIdAsync(int userId);
    Task<ApiResponse<bool>> ToggleUserStatusAsync(int userId);
    Task<ApiResponse<bool>> SetUserAdminStatusAsync(int userId, bool isAdmin);
}
