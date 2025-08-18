using manyasligida.Models;
using manyasligida.Models.DTOs;

namespace manyasligida.Services.Interfaces;

public interface IAccountService
{
    // Authentication
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> LogoutAsync();
    Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    
    // Profile Management
    Task<ApiResponse<UserResponse>> GetCurrentUserAsync();
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request);
    
    // Password Reset
    Task<ApiResponse<bool>> SendPasswordResetCodeAsync(string email);
    Task<ApiResponse<bool>> VerifyPasswordResetCodeAsync(string email, string resetCode);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request);
    
    // Email Verification
    Task<ApiResponse<bool>> VerifyEmailAsync(string email, string verificationCode);
    Task<ApiResponse<bool>> ResendVerificationCodeAsync(string email);
}
