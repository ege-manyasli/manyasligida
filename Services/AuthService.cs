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

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (user != null && VerifyPassword(password, user.Password))
            {
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
    }
}
