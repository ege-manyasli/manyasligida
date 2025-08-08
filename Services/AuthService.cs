using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using System.Security.Cryptography;
using System.Text;

namespace manyasligida.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (user != null && VerifyPassword(password, user.Password))
            {
                // Email doğrulaması kontrolü
                if (!user.EmailConfirmed)
                {
                    throw new InvalidOperationException("E-posta adresiniz henüz doğrulanmamış. Lütfen önce e-posta adresinizi doğrulayın.");
                }

                // Update last login
                user.LastLoginAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                SetCurrentUser(user);
                return user;
            }

            return null;
        }

        public async Task<User?> RegisterAsync(RegisterViewModel model)
        {
            if (await IsEmailExistsAsync(model.Email))
                return null;

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email.ToLower(),
                Phone = model.Phone,
                Password = HashPassword(model.Password),
                Address = model.Address,
                City = model.City,
                PostalCode = model.PostalCode,
                IsActive = true,
                IsAdmin = false,
                EmailConfirmed = false,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Email doğrulama kodu gönder
            await SendVerificationCodeAsync(user.Email);

            return user;
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
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var userIdString = session.GetString("UserId");
            var sessionId = session.GetString("SessionId");
            
            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(sessionId) || !int.TryParse(userIdString, out int userId))
                return null;

            // Session'ın geçerliliğini kontrol et
            var loginTimeString = session.GetString("LoginTime");
            if (!string.IsNullOrEmpty(loginTimeString) && DateTime.TryParse(loginTimeString, out DateTime loginTime))
            {
                // 30 dakikadan eski session'ları geçersiz kıl
                if (DateTime.UtcNow.Subtract(loginTime).TotalMinutes > 30)
                {
                    session.Clear();
                    return null;
                }
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> IsCurrentUserAdminAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.IsAdmin == true;
        }

        public void SetCurrentUser(User user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            // Önce session'ı temizle
            session.Clear();
            
            // Yeni kullanıcı bilgilerini set et
            session.SetString("UserId", user.Id.ToString());
            session.SetString("UserName", user.FullName);
            session.SetString("UserEmail", user.Email);
            session.SetString("IsAdmin", user.IsAdmin.ToString());
            session.SetString("LoginTime", DateTime.UtcNow.ToString("O"));
            session.SetString("SessionId", Guid.NewGuid().ToString());
        }

        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                // Session'ı tamamen temizle
                session.Clear();
                
                // Tüm session key'lerini manuel olarak temizle
                session.Remove("UserId");
                session.Remove("UserName");
                session.Remove("UserEmail");
                session.Remove("IsAdmin");
                session.Remove("LoginTime");
                session.Remove("SessionId");
                
                // Cookie'yi de temizle
                var response = _httpContextAccessor.HttpContext?.Response;
                if (response != null)
                {
                    response.Cookies.Delete(".ManyasliGida.Session");
                    response.Cookies.Delete("ASP.NET_SessionId");
                    
                    // Tüm cookie'leri temizle
                    var request = _httpContextAccessor.HttpContext?.Request;
                    if (request != null)
                    {
                        var cookies = request.Cookies.Keys;
                        foreach (var cookie in cookies)
                        {
                            response.Cookies.Delete(cookie);
                        }
                    }
                }
                
                // Session'ı commit et
                session.CommitAsync().Wait();
            }
        }

        private string GetSalt()
        {
            // Basit bir salt - production'da daha güvenli bir yöntem kullanılmalı
            return "ManyasliGida2024!";
        }

        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            try
            {
                // Önceki doğrulama kodlarını temizle
                var existingCodes = await _context.EmailVerifications
                    .Where(ev => ev.Email == email && !ev.IsUsed)
                    .ToListAsync();

                _context.EmailVerifications.RemoveRange(existingCodes);

                // Yeni doğrulama kodu oluştur
                var verificationCode = GenerateVerificationCode();
                var expiresAt = DateTime.Now.AddMinutes(10);

                var emailVerification = new EmailVerification
                {
                    Email = email,
                    VerificationCode = verificationCode,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = expiresAt,
                    IsUsed = false
                };

                _context.EmailVerifications.Add(emailVerification);
                await _context.SaveChangesAsync();

                // Email gönder
                return await _emailService.SendVerificationEmailAsync(email, verificationCode);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            try
            {
                // Email'i normalize et
                email = email.ToLower().Trim();
                code = code.Trim();

                var verification = await _context.EmailVerifications
                    .Where(ev => ev.Email.ToLower() == email && 
                                ev.VerificationCode.Trim() == code && 
                                !ev.IsUsed && 
                                ev.ExpiresAt > DateTime.Now)
                    .FirstOrDefaultAsync();

                if (verification == null)
                    return false;

                // Kullanıcıyı bul ve email'i doğrula
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email);
                if (user == null)
                    return false;

                user.EmailConfirmed = true;
                verification.IsUsed = true;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Email verification error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            return await SendVerificationCodeAsync(email);
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
