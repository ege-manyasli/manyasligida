using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace manyasligida.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private const int CacheExpirationMinutes = 30;

        public AuthService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger,
            IEmailService emailService,
            IMemoryCache cache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
        {
            try
            {
                _logger.LogInformation("Login attempt for: {Email}", email);

                // Check for too many login attempts
                var loginAttemptsKey = $"login_attempts_{email.ToLower()}";
                if (_cache.TryGetValue(loginAttemptsKey, out int attempts) && attempts >= 5)
                {
                    _logger.LogWarning("Too many login attempts for: {Email}", email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Çok fazla başarısız giriş denemesi. Lütfen 1 saat sonra tekrar deneyin."
                    };
                }

                // Kullanıcıyı veritabanından al
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

                if (user == null)
                {
                    await IncrementLoginAttemptsAsync(email);
                    _logger.LogWarning("Login failed - User not found: {Email}", email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "E-posta veya şifre hatalı."
                    };
                }

                // Şifreyi kontrol et
                if (!VerifyPassword(password, user.Password))
                {
                    await IncrementLoginAttemptsAsync(email);
                    _logger.LogWarning("Login failed - Invalid password for user: {Email}", email);
                    
                    if (!user.EmailConfirmed)
                    {
                        return new AuthResult
                        {
                            Success = false,
                            Message = "E-posta adresinizi doğrulamanız gerekmektedir. Lütfen e-posta kutunuzu kontrol edin."
                        };
                    }
                    
                    return new AuthResult
                    {
                        Success = false,
                        Message = "E-posta veya şifre hatalı."
                    };
                }

                // E-posta doğrulanmamışsa uyarı ver
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed - Email not confirmed: {Email}", email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "E-posta adresinizi doğrulamanız gerekmektedir. Lütfen e-posta kutunuzu kontrol edin."
                    };
                }

                // Clear login attempts on successful login
                _cache.Remove(loginAttemptsKey);

                // Son giriş zamanını güncelle
                user.LastLoginAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                // Clear user cache to refresh data
                var userCacheKey = $"user_{user.Id}";
                _cache.Remove(userCacheKey);

                // Claims oluştur
                var claims = CreateUserClaims(user);

                // Authentication cookie oluştur
                var authResult = await CreateAuthenticationCookieAsync(claims, rememberMe);

                if (authResult)
                {
                    _logger.LogInformation("Login successful for: {Email}", email);
                    return new AuthResult
                    {
                        Success = true,
                        Message = "Giriş başarılı!",
                        User = user,
                        Claims = claims
                    };
                }

                return new AuthResult
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin."
                };
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                _logger.LogInformation("Registration attempt for: {Email}", model.Email);

                // E-posta kontrolü
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed - Email already exists: {Email}", model.Email);
                    return new AuthResult
                    {
                        Success = false,
                        Message = "Bu e-posta adresi zaten kullanımda."
                    };
                }

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.ToLower().Trim(),
                    Phone = model.Phone.Trim(),
                    Password = HashPassword(model.Password),
                    Address = model.Address?.Trim(),
                    City = model.City?.Trim(),
                    PostalCode = model.PostalCode?.Trim(),
                    IsActive = true,
                    IsAdmin = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTimeHelper.NowTurkey,
                    UpdatedAt = DateTimeHelper.NowTurkey
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // E-posta doğrulama kodu gönder
                await SendEmailVerificationCodeAsync(user.Email);

                _logger.LogInformation("Registration successful for: {Email}", model.Email);
                return new AuthResult
                {
                    Success = true,
                    Message = "Kayıt başarılı! E-posta adresinize gönderilen doğrulama kodunu girin.",
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for: {Email}", model.Email);
                return new AuthResult
                {
                    Success = false,
                    Message = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin."
                };
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                await _httpContextAccessor.HttpContext?.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated != true)
                    return null;

                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return null;

                // Try to get from cache first
                var cacheKey = $"user_{userId}";
                if (_cache.TryGetValue(cacheKey, out User? cachedUser))
                {
                    return cachedUser;
                }

                // Get from database
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user != null)
                {
                    // Cache the user for 30 minutes
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(CacheExpirationMinutes));
                    _cache.Set(cacheKey, user, cacheOptions);
                }

                return user;
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

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ManyasliGida2024!"));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
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

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                if (user == null) return false;

                if (!VerifyPassword(currentPassword, user.Password))
                    return false;

                user.Password = HashPassword(newPassword);
                user.UpdatedAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                // Clear user cache after password change
                var cacheKey = $"user_{userId}";
                _cache.Remove(cacheKey);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCodeAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
                if (user == null) return false;

                var resetCode = GenerateVerificationCode();
                var expiry = DateTimeHelper.NowTurkey.AddMinutes(15);

                user.PasswordResetCode = resetCode;
                user.PasswordResetCodeExpiry = expiry;
                user.UpdatedAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();

                await _emailService.SendPasswordResetEmailAsync(email, resetCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> VerifyPasswordResetCodeAsync(string email, string resetCode)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
                if (user == null) return false;

                return user.PasswordResetCode == resetCode && 
                       user.PasswordResetCodeExpiry > DateTimeHelper.NowTurkey;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password reset code for {Email}", email);
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string verificationCode)
        {
            try
            {
                var verification = await _context.EmailVerifications
                    .FirstOrDefaultAsync(v => v.Email.ToLower() == email.ToLower() && 
                                             v.VerificationCode == verificationCode && 
                                             !v.IsUsed && 
                                             v.ExpiresAt > DateTimeHelper.NowTurkey);

                if (verification == null) return false;

                // Kullanıcıyı doğrula
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    user.UpdatedAt = DateTimeHelper.NowTurkey;
                }

                // Doğrulama kodunu kullanıldı olarak işaretle
                verification.IsUsed = true;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", email);
                return false;
            }
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user == null) return false;

                if (user.EmailConfirmed) return true;

                await SendEmailVerificationCodeAsync(email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendEmailVerificationCodeAsync(string email)
        {
            try
            {
                var code = GenerateVerificationCode();
                var expiry = DateTimeHelper.NowTurkey.AddMinutes(15);

                var verification = new EmailVerification
                {
                    Email = email.ToLower(),
                    VerificationCode = code,
                    ExpiresAt = expiry,
                    IsUsed = false,
                    CreatedAt = DateTimeHelper.NowTurkey
                };

                _context.EmailVerifications.Add(verification);
                await _context.SaveChangesAsync();

                await _emailService.SendEmailVerificationAsync(email, code);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification to {Email}", email);
                return false;
            }
        }

        // Private helper methods
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

        private async Task IncrementLoginAttemptsAsync(string email)
        {
            var loginAttemptsKey = $"login_attempts_{email.ToLower()}";
            if (_cache.TryGetValue(loginAttemptsKey, out int attempts))
            {
                _cache.Set(loginAttemptsKey, attempts + 1, TimeSpan.FromHours(1)); // 1 saat boyunca artır
            }
            else
            {
                _cache.Set(loginAttemptsKey, 1, TimeSpan.FromHours(1)); // 1 saat boyunca artır
            }
        }

        private static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
