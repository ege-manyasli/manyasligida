using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace manyasligida.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccountService> _logger;
        private readonly IEmailService _emailService;

        public AccountService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccountService> logger,
            IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for: {Email}", request.Email);

                // Find user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("Login failed - User not found: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.FailureResult("E-posta veya şifre hatalı");
                }

                // Verify password
                if (!VerifyPassword(request.Password, user.Password))
                {
                    _logger.LogWarning("Login failed - Invalid password for user: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.FailureResult("E-posta veya şifre hatalı");
                }

                // Check email confirmation
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed - Email not confirmed: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.FailureResult("E-posta adresinizi doğrulamanız gerekmektedir");
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Create claims
                var claims = CreateUserClaims(user);

                // Create authentication cookie
                var authResult = await CreateAuthenticationCookieAsync(claims, request.RememberMe);

                if (authResult)
                {
                    _logger.LogInformation("Login successful for: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.SuccessResult(new LoginResponse
                    {
                        User = MapToUserResponse(user),
                        Token = "cookie-based-auth" // Cookie tabanlı authentication kullanıyoruz
                    });
                }

                return ApiResponse<LoginResponse>.FailureResult("Giriş sırasında bir hata oluştu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", request.Email);
                return ApiResponse<LoginResponse>.FailureResult("Giriş sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", request.Email);

                // Check if email exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed - Email already exists: {Email}", request.Email);
                    return ApiResponse<RegisterResponse>.FailureResult("Bu e-posta adresi zaten kullanımda");
                }

                // Create new user
                var user = new User
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    Phone = request.Phone.Trim(),
                    Password = HashPassword(request.Password),
                    Address = request.Address?.Trim(),
                    City = request.City?.Trim(),
                    PostalCode = request.PostalCode?.Trim(),
                    IsActive = true,
                    IsAdmin = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send email verification
                await SendEmailVerificationCodeAsync(user.Email);

                _logger.LogInformation("Registration successful for: {Email}", request.Email);
                return ApiResponse<RegisterResponse>.SuccessResult(new RegisterResponse
                {
                    User = MapToUserResponse(user),
                    Message = "Kayıt başarılı! E-posta adresinize gönderilen doğrulama kodunu girin."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for: {Email}", request.Email);
                return ApiResponse<RegisterResponse>.FailureResult("Kayıt sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return ApiResponse<bool>.FailureResult("HTTP context bulunamadı");
                }

                // Clear authentication cookie
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Clear all session data
                httpContext.Session.Clear();

                _logger.LogInformation("User logged out successfully");

                return ApiResponse<bool>.SuccessResult(true, "Çıkış başarılı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ApiResponse<bool>.FailureResult("Çıkış sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            try
            {
                var currentUser = await GetCurrentUserInternalAsync();
                if (currentUser == null)
                {
                    return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
                }

                // Verify current password
                if (!VerifyPassword(request.CurrentPassword, currentUser.Password))
                {
                    return ApiResponse<bool>.FailureResult("Mevcut şifre hatalı");
                }

                // Update password
                currentUser.Password = HashPassword(request.NewPassword);
                currentUser.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", currentUser.Id);

                return ApiResponse<bool>.SuccessResult(true, "Şifre başarıyla değiştirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return ApiResponse<bool>.FailureResult("Şifre değiştirme sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<UserResponse>> GetCurrentUserAsync()
        {
            try
            {
                var user = await GetCurrentUserInternalAsync();
                if (user == null)
                {
                    return ApiResponse<UserResponse>.FailureResult("Kullanıcı bulunamadı");
                }

                return ApiResponse<UserResponse>.SuccessResult(MapToUserResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return ApiResponse<UserResponse>.FailureResult("Kullanıcı bilgileri alınamadı");
            }
        }

        public async Task<ApiResponse<UserResponse>> UpdateProfileAsync(UpdateProfileRequest request)
        {
            try
            {
                var user = await GetCurrentUserInternalAsync();
                if (user == null)
                {
                    return ApiResponse<UserResponse>.FailureResult("Kullanıcı bulunamadı");
                }

                // Update user properties
                user.FirstName = request.FirstName.Trim();
                user.LastName = request.LastName.Trim();
                user.Phone = request.Phone.Trim();
                user.Address = request.Address?.Trim();
                user.City = request.City?.Trim();
                user.PostalCode = request.PostalCode?.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile updated for user {UserId}", user.Id);

                return ApiResponse<UserResponse>.SuccessResult(MapToUserResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return ApiResponse<UserResponse>.FailureResult("Profil güncelleme sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> SendPasswordResetCodeAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
                if (user == null)
                {
                    return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
                }

                var resetCode = GenerateVerificationCode();
                var expiry = DateTime.UtcNow.AddMinutes(15);

                user.PasswordResetCode = resetCode;
                user.PasswordResetCodeExpiry = expiry;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _emailService.SendPasswordResetEmailAsync(email, resetCode);

                return ApiResponse<bool>.SuccessResult(true, "Şifre sıfırlama kodu e-posta adresinize gönderildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to {Email}", email);
                return ApiResponse<bool>.FailureResult("Şifre sıfırlama kodu gönderilemedi");
            }
        }

        public async Task<ApiResponse<bool>> VerifyPasswordResetCodeAsync(string email, string resetCode)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
                if (user == null)
                {
                    return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
                }

                var isValid = user.PasswordResetCode == resetCode && 
                             user.PasswordResetCodeExpiry > DateTime.UtcNow;

                return ApiResponse<bool>.SuccessResult(isValid, isValid ? "Kod doğrulandı" : "Geçersiz veya süresi dolmuş kod");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password reset code for {Email}", email);
                return ApiResponse<bool>.FailureResult("Kod doğrulama sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);
                if (user == null)
                {
                    return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
                }

                // Verify reset code
                if (user.PasswordResetCode != request.ResetCode || 
                    user.PasswordResetCodeExpiry <= DateTime.UtcNow)
                {
                    return ApiResponse<bool>.FailureResult("Geçersiz veya süresi dolmuş kod");
                }

                // Update password
                user.Password = HashPassword(request.NewPassword);
                user.PasswordResetCode = null;
                user.PasswordResetCodeExpiry = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password reset for user {UserId}", user.Id);

                return ApiResponse<bool>.SuccessResult(true, "Şifre başarıyla sıfırlandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                return ApiResponse<bool>.FailureResult("Şifre sıfırlama sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> VerifyEmailAsync(string email, string verificationCode)
        {
            try
            {
                var verification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(v => v.Email.ToLower() == email.ToLower() && 
                                             v.VerificationCode == verificationCode && 
                                             !v.IsUsed && 
                                             v.ExpiresAt > DateTime.UtcNow);

                if (verification == null)
                {
                    return ApiResponse<bool>.FailureResult("Geçersiz veya süresi dolmuş doğrulama kodu");
                }

                // Verify user
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                // Mark verification as used
                verification.IsUsed = true;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResult(true, "E-posta adresi başarıyla doğrulandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", email);
                return ApiResponse<bool>.FailureResult("E-posta doğrulama sırasında bir hata oluştu");
            }
        }

        public async Task<ApiResponse<bool>> ResendVerificationCodeAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null)
                {
                    return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
                }

                if (user.EmailConfirmed)
                {
                    return ApiResponse<bool>.SuccessResult(true, "E-posta adresi zaten doğrulanmış");
                }

                await SendEmailVerificationCodeAsync(email);

                return ApiResponse<bool>.SuccessResult(true, "Yeni doğrulama kodu e-posta adresinize gönderildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code to {Email}", email);
                return ApiResponse<bool>.FailureResult("Doğrulama kodu gönderilemedi");
            }
        }

        // Private helper methods
        private async Task<User?> GetCurrentUserInternalAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                    return null;

                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return null;

                return await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        private List<Claim> CreateUserClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("UserId", user.Id.ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
                new Claim("Email", user.Email),
                new Claim("Phone", user.Phone ?? ""),
                new Claim("City", user.City ?? ""),
                new Claim("Address", user.Address ?? "")
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            claims.Add(new Claim(ClaimTypes.Role, "User"));

            return claims;
        }

        private async Task<bool> CreateAuthenticationCookieAsync(List<Claim> claims, bool rememberMe)
        {
            try
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2),
                    AllowRefresh = true
                };

                await _httpContextAccessor.HttpContext?.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating authentication cookie");
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ManyasliGida2024!"));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                // Plain text kontrolü (migration için)
                if (password == hashedPassword) return true;
                
                // Hash kontrolü
                var hashedInput = HashPassword(password);
                return hashedInput == hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPassword");
                return false;
            }
        }

        private async Task SendEmailVerificationCodeAsync(string email)
        {
            try
            {
                var code = GenerateVerificationCode();
                var expiry = DateTime.UtcNow.AddMinutes(15);

                var verification = new EmailVerification
                {
                    Email = email.ToLower(),
                    VerificationCode = code,
                    ExpiresAt = expiry,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmailVerifications.Add(verification);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailVerificationAsync(email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification to {Email}", email);
            }
        }

        private static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private static UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City,
                PostalCode = user.PostalCode,
                IsAdmin = user.IsAdmin,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}
