using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using System.Security.Cryptography;
using System.Text;

namespace manyasligida.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISessionManager _sessionManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, ISessionManager sessionManager, IEmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _sessionManager = sessionManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", email);

                // Normalize inputs
                email = (email ?? string.Empty).Trim().ToLowerInvariant();
                password = (password ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Login attempt with empty email or password");
                    return null;
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

                _logger.LogInformation("User found: {UserFound}, Email: {Email}, IsActive: {IsActive}", 
                    user != null, email, user?.IsActive);

                if (user != null)
                {
                    var passwordValid = VerifyPassword(password, user.Password);
                    _logger.LogInformation("Password validation result: {PasswordValid} for user {UserId}", passwordValid, user.Id);
                    _logger.LogInformation("Input password length: {InputLength}, Stored password length: {StoredLength}", password.Length, user.Password.Length);
                    _logger.LogInformation("Stored password starts with: {PasswordStart}", user.Password.Substring(0, Math.Min(20, user.Password.Length)));
                    
                    // MANUAL TEST: Try direct hash comparison
                    var manualHash = HashPassword(password);
                    var manualMatch = string.Equals(manualHash, user.Password, StringComparison.Ordinal);
                    _logger.LogInformation("Manual hash test - Generated: {Generated}, Match: {Match}", manualHash.Substring(0, Math.Min(20, manualHash.Length)), manualMatch);
                    
                    if (passwordValid)
                    {
                        // Email doğrulaması kontrolü - ZORUNLU
                        if (!user.EmailConfirmed)
                        {
                            _logger.LogWarning("Login denied for unverified email: {Email}", user.Email);
                            return null; // E-posta doğrulanmamış, giriş yapamaz
                        }

                        // Auto-migrate password to current hash format if needed
                        var needsMigration = false;
                        if (string.Equals(user.Password, password, StringComparison.Ordinal))
                        {
                            // Plain text password - migrate to hash
                            needsMigration = true;
                            _logger.LogInformation("Migrating plain text password to hash for user {UserId}", user.Id);
                        }
                        else if (!user.Password.StartsWith(HashPassword(password).Substring(0, 10)))
                        {
                            // Legacy hash format - check and migrate
                            using var sha256 = SHA256.Create();
                            var legacyHash1 = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
                            var legacyHash2 = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "salt")));
                            
                            if (string.Equals(legacyHash1, user.Password, StringComparison.Ordinal) || 
                                string.Equals(legacyHash2, user.Password, StringComparison.Ordinal))
                            {
                                needsMigration = true;
                                _logger.LogInformation("Migrating legacy hash to current format for user {UserId}", user.Id);
                            }
                        }
                        
                        if (needsMigration)
                        {
                            user.Password = HashPassword(password);
                            _logger.LogInformation("Password migrated to current hash format for user {UserId}", user.Id);
                        }

                        // Update last login
                        user.LastLoginAt = DateTimeHelper.NowTurkey;
                        await _context.SaveChangesAsync();

                        // Enforce single active session per user
                        await _sessionManager.ForceLogoutOtherSessionsAsync(user.Id);

                        // Create new session using SessionManager
                        var sessionCreated = await _sessionManager.CreateUserSessionAsync(user);
                        if (!sessionCreated)
                        {
                            _logger.LogError("Failed to create session for user {UserId}", user.Id);
                            return null;
                        }

                        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
                        return user;
                    }
                    else
                    {
                        _logger.LogWarning("Password validation failed for user {UserId}", user.Id);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", email);
                throw;
            }
        }

        public async Task<User?> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Normalize inputs
                model.Email = model.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                model.Password = model.Password?.Trim() ?? string.Empty;

                if (await IsEmailExistsAsync(model.Email))
                    return null;

                var hashedPassword = HashPassword(model.Password);
                _logger.LogInformation("Register - Original password length: {OrigLen}, Hashed length: {HashLen}", model.Password.Length, hashedPassword.Length);
                _logger.LogInformation("Register - Hashed password preview: {HashPreview}", hashedPassword.Substring(0, Math.Min(20, hashedPassword.Length)));
                
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email.ToLower(),
                    Phone = model.Phone,
                    Password = hashedPassword,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    IsActive = true,
                    IsAdmin = false,
                    EmailConfirmed = false,
                    CreatedAt = DateTimeHelper.NowTurkey
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Email doğrulama kodu gönder
                await SendVerificationCodeAsync(user.Email);

                _logger.LogInformation("User registered successfully: {Email}", user.Email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email {Email}", model.Email);
                throw;
            }
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + GetSalt()));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                _logger.LogInformation("VerifyPassword called - Input length: {InputLen}, Stored length: {StoredLen}", password?.Length ?? 0, hashedPassword?.Length ?? 0);
                
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                {
                    _logger.LogWarning("Empty password or hash provided");
                    return false;
                }
                
                // 1. Plain text check first (for existing users)
                if (string.Equals(password, hashedPassword, StringComparison.Ordinal))
                {
                    _logger.LogInformation("Plain text password match - SUCCESS");
                    return true;
                }
                
                // 2. Current hash format check
                var currentHash = HashPassword(password);
                if (string.Equals(currentHash, hashedPassword, StringComparison.Ordinal))
                {
                    _logger.LogInformation("Current hash format match - SUCCESS");
                    return true;
                }
                
                _logger.LogWarning("Password verification FAILED - Current hash: {CurrentHash}, Stored: {StoredHash}", 
                    currentHash?.Substring(0, Math.Min(15, currentHash.Length)), 
                    hashedPassword?.Substring(0, Math.Min(15, hashedPassword.Length)));
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyPassword");
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                return await _sessionManager.GetCurrentUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user from AuthService");
                return null;
            }
        }

        public async Task<bool> IsCurrentUserAdminAsync()
        {
            return await _sessionManager.IsCurrentUserAdminAsync();
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            return await _sessionManager.IsUserLoggedInAsync();
        }

        public async Task<bool> IsUserAdminAsync()
        {
            return await _sessionManager.IsUserAdminAsync();
        }

        public async Task<bool> ValidateSessionAsync()
        {
            return await _sessionManager.ValidateSessionAsync();
        }

        public async Task<bool> ExtendSessionAsync()
        {
            return await _sessionManager.ExtendSessionAsync();
        }

        public async Task<bool> ForceLogoutOtherSessionsAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return false;

            return await _sessionManager.ForceLogoutOtherSessionsAsync(currentUser.Id);
        }

        public async Task<int> GetActiveSessionCountAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
                return 0;

            return await _sessionManager.GetActiveSessionCountAsync(currentUser.Id);
        }

        public async Task Logout()
        {
            try
            {
                await _sessionManager.InvalidateSessionAsync();
                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<bool> RefreshSessionAsync()
        {
            return await _sessionManager.RefreshSessionAsync();
        }

        public async Task<DateTime?> GetSessionExpiryAsync()
        {
            return await _sessionManager.GetSessionExpiryAsync();
        }

        public async Task<bool> IsSessionExpiredAsync()
        {
            return await _sessionManager.IsSessionExpiredAsync();
        }

        public async Task CleanupExpiredSessionsAsync()
        {
            await _sessionManager.CleanupExpiredSessionsAsync();
        }

        public void SetCurrentUser(User user)
        {
            // This method is used to update the current user in the session
            // The actual session update is handled by the SessionManager
            _logger.LogInformation("Setting current user: {UserId}", user.Id);
        }

        public async Task<bool> VerifyEmailAsync(string email, string verificationCode)
        {
            try
            {
                email = (email ?? string.Empty).Trim().ToLowerInvariant();
                verificationCode = (verificationCode ?? string.Empty).Trim();
                
                _logger.LogInformation("Email verification attempt - Email: {Email}, Code: {Code}", email, verificationCode);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
                if (user == null) 
                {
                    _logger.LogWarning("User not found for email: {Email}", email);
                    return false;
                }

                // Önce kullanıcının zaten doğrulanmış olup olmadığını kontrol et
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already verified for user: {Email}", email);
                    return true; // Zaten doğrulanmış, başarılı döndür
                }

                var record = await _context.EmailVerifications
                    .FirstOrDefaultAsync(v => v.Email.ToLower() == email && !v.IsUsed);
                
                if (record == null) 
                {
                    _logger.LogWarning("No verification record found for email: {Email}", email);
                    
                    // Eğer kullanılmış kayıt varsa, kullanıcıyı bilgilendir
                    var usedRecord = await _context.EmailVerifications
                        .FirstOrDefaultAsync(v => v.Email.ToLower() == email && v.IsUsed);
                    if (usedRecord != null)
                    {
                        _logger.LogWarning("Used verification record found for email: {Email}. Code was already used.", email);
                    }
                    return false;
                }
                
                _logger.LogInformation("Verification record found - Code: {StoredCode}, Expires: {ExpiresAt}, IsUsed: {IsUsed}", 
                    record.VerificationCode, record.ExpiresAt, record.IsUsed);
                
                if (record.ExpiresAt <= DateTimeHelper.NowTurkey) 
                {
                    _logger.LogWarning("Verification code expired for email: {Email}. Expires: {ExpiresAt}, Now: {Now}", 
                        email, record.ExpiresAt, DateTimeHelper.NowTurkey);
                    return false;
                }
                
                if (!string.Equals(record.VerificationCode, verificationCode, StringComparison.Ordinal)) 
                {
                    _logger.LogWarning("Verification code mismatch for email: {Email}. Expected: {Expected}, Provided: {Provided}", 
                        email, record.VerificationCode, verificationCode);
                    return false;
                }

                user.EmailConfirmed = true;
                user.UpdatedAt = DateTimeHelper.NowTurkey;
                record.IsUsed = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Email verified successfully for user: {Email}", email);
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
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                if (user != null)
                {
                    // Send new verification code
                    await SendVerificationCodeAsync(email);
                    _logger.LogInformation("Verification code resent to: {Email}", email);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
                if (user == null) return false;

                // Verify current password
                if (!VerifyPassword(currentPassword, user.Password)) return false;

                user.Password = HashPassword(newPassword);
                user.UpdatedAt = DateTimeHelper.NowTurkey;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Password changed for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return false;
            }
        }

        private string GetSalt()
        {
            // Basit bir salt - production'da daha güvenli bir yöntem kullanılmalı
            // Admin password hash: sNaWAMfPr1AQUDPjD9iKHB3Jc+Ky6BbhLPYBvU4hwjI=
            // Bu hash "admin123" + "ManyasliGida2024!" ile oluşturulmuş
            return "ManyasliGida2024!";
        }

        private async Task SendVerificationCodeAsync(string email)
        {
            try
            {
                // Email doğrulama kodu gönderme işlemi
                var verificationCode = GenerateVerificationCode();
                
                // Upsert verification record
                var existing = await _context.EmailVerifications.FirstOrDefaultAsync(v => v.Email.ToLower() == email.ToLower());
                if (existing == null)
                {
                    _context.EmailVerifications.Add(new EmailVerification
                    {
                        Email = email.ToLower(),
                        VerificationCode = verificationCode,
                        CreatedAt = DateTimeHelper.NowTurkey,
                        ExpiresAt = DateTimeHelper.GetEmailVerificationExpiry(15), // 15 dakika Türkiye saati
                        IsUsed = false
                    });
                }
                else
                {
                    existing.VerificationCode = verificationCode;
                    existing.ExpiresAt = DateTimeHelper.GetEmailVerificationExpiry(15); // 15 dakika Türkiye saati
                    existing.IsUsed = false;
                }

                await _context.SaveChangesAsync();

                // Send via email service
                await _emailService.SendVerificationEmailAsync(email, verificationCode);
                _logger.LogInformation("Verification code sent to {Email}: {Code}", email, verificationCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code to {Email}", email);
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
