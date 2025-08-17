using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Models.DTOs;
using manyasligida.Services.Interfaces;

namespace manyasligida.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountService> _logger;
    private readonly IConfiguration _configuration;

    public AccountService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        IEmailService emailService,
        ILogger<AccountService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new AuthResponse { Success = false, Message = "E-posta ve şifre gereklidir" };
            }

            // Find user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found for email {Email}", request.Email);
                return new AuthResponse { Success = false, Message = "E-posta veya şifre hatalı" };
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.Password))
            {
                _logger.LogWarning("Login failed: Invalid password for user {UserId}", user.Id);
                return new AuthResponse { Success = false, Message = "E-posta veya şifre hatalı" };
            }

            // Auto-confirm email for existing users
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                _logger.LogInformation("Auto-confirming email for existing user {UserId}", user.Id);
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Create session
            await CreateUserSessionAsync(user);

            // Create authentication cookie
            await SignInUserAsync(user, request.RememberMe);

            _logger.LogInformation("User {UserId} logged in successfully", user.Id);

            return new AuthResponse
            {
                Success = true,
                Message = "Giriş başarılı",
                User = MapToUserResponse(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Giriş sırasında bir hata oluştu" };
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

            // Check if email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (existingUser != null)
            {
                return new AuthResponse { Success = false, Message = "Bu e-posta adresi zaten kullanılıyor" };
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
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Send email verification
            await SendEmailVerificationCodeAsync(user.Email);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            return new AuthResponse
            {
                Success = true,
                Message = "Kayıt başarılı. Lütfen e-posta adresinizi doğrulayın.",
                User = MapToUserResponse(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", request.Email);
            return new AuthResponse { Success = false, Message = "Kayıt sırasında bir hata oluştu" };
        }
    }

    public async Task<ApiResponse<bool>> LogoutAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return ApiResponse<bool>.FailureResult("HTTP bağlamı bulunamadı");

            // Clear session
            var sessionId = httpContext.Session.GetString("SessionId");
            if (!string.IsNullOrEmpty(sessionId))
            {
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);
                if (session != null)
                {
                    session.IsActive = false;
                    await _context.SaveChangesAsync();
                }
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

            return ApiResponse<UserResponse>.SuccessResult(MapToUserResponse(user), "Profil başarıyla güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return ApiResponse<UserResponse>.FailureResult("Profil güncellenirken bir hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> SendEmailVerificationAsync(string email)
    {
        try
        {
            await SendEmailVerificationCodeAsync(email);
            return ApiResponse<bool>.SuccessResult(true, "Doğrulama kodu gönderildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email verification");
            return ApiResponse<bool>.FailureResult("E-posta doğrulama kodu gönderilemedi");
        }
    }

    public async Task<ApiResponse<bool>> VerifyEmailAsync(EmailVerificationRequest request)
    {
        try
        {
            var verification = await _context.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email.ToLower() == request.Email.ToLower() 
                                    && v.VerificationCode == request.VerificationCode 
                                    && !v.IsUsed 
                                    && v.ExpiresAt > DateTime.UtcNow);

            if (verification == null)
            {
                return ApiResponse<bool>.FailureResult("Geçersiz veya süresi dolmuş doğrulama kodu");
            }

            // Mark verification as used
            verification.IsUsed = true;

            // Update user email confirmation
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (user != null)
            {
                user.EmailConfirmed = true;
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for {Email}", request.Email);

            return ApiResponse<bool>.SuccessResult(true, "E-posta başarıyla doğrulandı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return ApiResponse<bool>.FailureResult("E-posta doğrulama sırasında bir hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> ResendVerificationCodeAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
            }

            if (user.EmailConfirmed)
            {
                return ApiResponse<bool>.FailureResult("E-posta zaten doğrulanmış");
            }

            await SendEmailVerificationCodeAsync(email);

            return ApiResponse<bool>.SuccessResult(true, "Doğrulama kodu yeniden gönderildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification code");
            return ApiResponse<bool>.FailureResult("Doğrulama kodu gönderilemedi");
        }
    }

    // Additional methods to complete interface implementation...
    public async Task<ApiResponse<bool>> DeleteAccountAsync()
    {
        try
        {
            var user = await GetCurrentUserInternalAsync();
            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
            }

            // Soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log out user
            await LogoutAsync();

            _logger.LogInformation("Account deactivated for user {UserId}", user.Id);

            return ApiResponse<bool>.SuccessResult(true, "Hesap başarıyla deaktivate edildi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting account");
            return ApiResponse<bool>.FailureResult("Hesap silme sırasında bir hata oluştu");
        }
    }

    public async Task<ApiResponse<bool>> ValidateSessionAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return ApiResponse<bool>.FailureResult("HTTP bağlamı bulunamadı");

            var sessionId = httpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                return ApiResponse<bool>.FailureResult("Oturum bulunamadı");
            }

            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session == null || session.ExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponse<bool>.FailureResult("Oturum geçersiz veya süresi dolmuş");
            }

            // Extend session
            session.LastActivity = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddHours(2);
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, "Oturum geçerli");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session");
            return ApiResponse<bool>.FailureResult("Oturum doğrulama hatası");
        }
    }

    public async Task<ApiResponse<bool>> ExtendSessionAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return ApiResponse<bool>.FailureResult("HTTP bağlamı bulunamadı");

            var sessionId = httpContext.Session.GetString("SessionId");
            if (string.IsNullOrEmpty(sessionId)) return ApiResponse<bool>.FailureResult("Oturum bulunamadı");

            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (session != null)
            {
                session.LastActivity = DateTime.UtcNow;
                session.ExpiresAt = DateTime.UtcNow.AddHours(2);
                await _context.SaveChangesAsync();
            }

            return ApiResponse<bool>.SuccessResult(true, "Oturum uzatıldı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session");
            return ApiResponse<bool>.FailureResult("Oturum uzatma hatası");
        }
    }

    public async Task<ApiResponse<int>> GetActiveSessionCountAsync()
    {
        try
        {
            var user = await GetCurrentUserInternalAsync();
            if (user == null) return ApiResponse<int>.FailureResult("Kullanıcı bulunamadı");

            var count = await _context.UserSessions
                .CountAsync(s => s.UserId == user.Id && s.IsActive && s.ExpiresAt > DateTime.UtcNow);

            return ApiResponse<int>.SuccessResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active session count");
            return ApiResponse<int>.FailureResult("Aktif oturum sayısı alınamadı");
        }
    }

    public async Task<ApiResponse<bool>> ForceLogoutOtherSessionsAsync()
    {
        try
        {
            var user = await GetCurrentUserInternalAsync();
            if (user == null) return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");

            var currentSessionId = _httpContextAccessor.HttpContext?.Session.GetString("SessionId");

            var otherSessions = await _context.UserSessions
                .Where(s => s.UserId == user.Id && s.IsActive && s.SessionId != currentSessionId)
                .ToListAsync();

            foreach (var session in otherSessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Forced logout other sessions for user {UserId}", user.Id);

            return ApiResponse<bool>.SuccessResult(true, "Diğer oturumlar sonlandırıldı");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing logout other sessions");
            return ApiResponse<bool>.FailureResult("Diğer oturumlar sonlandırılamadı");
        }
    }

    // Admin functions
    public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page = 1, int pageSize = 50)
    {
        try
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userResponses = users.Select(MapToUserResponse).ToList();

            return ApiResponse<List<UserResponse>>.SuccessResult(userResponses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return ApiResponse<List<UserResponse>>.FailureResult("Kullanıcılar alınamadı");
        }
    }

    public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserResponse>.FailureResult("Kullanıcı bulunamadı");
            }

            return ApiResponse<UserResponse>.SuccessResult(MapToUserResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id {UserId}", userId);
            return ApiResponse<UserResponse>.FailureResult("Kullanıcı alınamadı");
        }
    }

    public async Task<ApiResponse<bool>> ToggleUserStatusAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, $"Kullanıcı durumu {(user.IsActive ? "aktif" : "pasif")} olarak güncellendi");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            return ApiResponse<bool>.FailureResult("Kullanıcı durumu değiştirilemedi");
        }
    }

    public async Task<ApiResponse<bool>> SetUserAdminStatusAsync(int userId, bool isAdmin)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return ApiResponse<bool>.FailureResult("Kullanıcı bulunamadı");
            }

            user.IsAdmin = isAdmin;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResult(true, $"Admin yetkisi {(isAdmin ? "verildi" : "kaldırıldı")}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting admin status");
            return ApiResponse<bool>.FailureResult("Admin yetkisi değiştirilemedi");
        }
    }

    // Private helper methods
    private async Task<User?> GetCurrentUserInternalAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return null;

        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "ManyasliGida2024!"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        // Check plain text first (for migration)
        if (password == hashedPassword) return true;
        
        // Check hashed password
        var hashedInput = HashPassword(password);
        return hashedInput == hashedPassword;
    }

    private async Task CreateUserSessionAsync(User user)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var sessionId = Guid.NewGuid().ToString();
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var session = new UserSession
        {
            SessionId = sessionId,
            UserId = user.Id,
            DeviceInfo = GetDeviceInfo(userAgent),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = "Unknown", // Could be enhanced with IP geolocation
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(2),
            LastActivity = DateTime.UtcNow,
            IsActive = true
        };

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        // Store session ID
        httpContext.Session.SetString("SessionId", sessionId);
        httpContext.Session.SetString("UserId", user.Id.ToString());
        httpContext.Session.SetString("UserName", user.FullName);
        httpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
    }

    private async Task SignInUserAsync(User user, bool rememberMe)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(2)
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);
    }

    private async Task SendEmailVerificationCodeAsync(string email)
    {
        var code = GenerateVerificationCode();
        var expiry = DateTimeHelper.NowTurkey.AddMinutes(15); // Türkiye zamanı kullan

        // Save verification code
        var verification = new EmailVerification
        {
            Email = email.ToLower(),
            VerificationCode = code,
            ExpiresAt = expiry,
            IsUsed = false,
            CreatedAt = DateTimeHelper.NowTurkey // Türkiye zamanı kullan
        };

        _context.EmailVerifications.Add(verification);
        await _context.SaveChangesAsync();

        // Send email (implement based on your email service)
        try
        {
            await _emailService.SendEmailVerificationAsync(email, code);
            _logger.LogInformation("Verification email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", email);
        }
    }

    private static string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private static string GetDeviceInfo(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        // Simple device detection
        if (userAgent.Contains("Mobile")) return "Mobile";
        if (userAgent.Contains("Tablet")) return "Tablet";
        return "Desktop";
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Address = user.Address,
            City = user.City,
            PostalCode = user.PostalCode,
            IsActive = user.IsActive,
            IsAdmin = user.IsAdmin,
            EmailConfirmed = user.EmailConfirmed,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
