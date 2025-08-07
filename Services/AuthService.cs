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
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return null;

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

            session.SetString("UserId", user.Id.ToString());
            session.SetString("UserName", user.FullName);
            session.SetString("UserEmail", user.Email);
            session.SetString("IsAdmin", user.IsAdmin.ToString());
        }

        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Clear();
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
                var verification = await _context.EmailVerifications
                    .Where(ev => ev.Email == email && ev.VerificationCode == code && !ev.IsUsed && ev.ExpiresAt > DateTime.Now)
                    .FirstOrDefaultAsync();

                if (verification == null)
                    return false;

                // Kullanıcıyı bul ve email'i doğrula
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                    return false;

                user.EmailConfirmed = true;
                verification.IsUsed = true;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
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
