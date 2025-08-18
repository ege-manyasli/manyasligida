using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using manyasligida.Models;
using manyasligida.Services;
using manyasligida.Data;
using System.Security.Claims;

namespace manyasligida.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthService authService,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                _logger.LogInformation("Login attempt for: {Email}", model.Email);

                var authResult = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe);

                if (authResult.Success)
                {
                    _logger.LogInformation("Login successful for: {Email}", model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", authResult.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for: {Email}", model.Email);
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed for registration: {Email}", model.Email);
                    return View(model);
                }

                _logger.LogInformation("Registration attempt for: {Email}", model.Email);

                var authResult = await _authService.RegisterAsync(model);

                if (authResult.Success)
                {
                    _logger.LogInformation("Registration successful for: {Email}", model.Email);
                    TempData["Success"] = authResult.Message;
                    return RedirectToAction("VerifyEmail", new { email = model.Email });
                }
                else
                {
                    ModelState.AddModelError("", authResult.Message);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for: {Email}", model.Email);
                ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                _logger.LogInformation("User logged out successfully");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var emailSent = await _authService.SendPasswordResetCodeAsync(model.Email);
                    
                    if (emailSent)
                    {
                        _logger.LogInformation("Password reset email sent successfully: {Email}", model.Email);
                        TempData["Success"] = "Şifre sıfırlama kodu e-posta adresinize gönderildi.";
                        TempData["Email"] = model.Email;
                        return RedirectToAction("VerifyResetCode");
                    }
                    else
                    {
                        _logger.LogWarning("Password reset email failed: {Email}", model.Email);
                        // Don't reveal if email exists or not for security
                        TempData["Success"] = "Şifre sıfırlama kodu e-posta adresinize gönderildi.";
                        TempData["Email"] = model.Email;
                        return RedirectToAction("VerifyResetCode");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ForgotPassword");
                    ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyResetCode()
        {
            var email = TempData["Email"]?.ToString();
            var model = new VerifyResetCodeViewModel
            {
                Email = email ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var isValid = await _authService.VerifyPasswordResetCodeAsync(model.Email, model.VerificationCode);

                    if (isValid)
                    {
                        // Store email in session for change password step
                        HttpContext.Session.SetString("ResetEmail", model.Email);
                        
                        TempData["Success"] = "Kod doğrulandı. Şimdi yeni şifrenizi belirleyin.";
                        return RedirectToAction("ChangePassword");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Geçersiz veya süresi dolmuş doğrulama kodu.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in VerifyResetCode");
                    ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Oturum süresi dolmuş. Lütfen tekrar deneyin.";
                return RedirectToAction("ForgotPassword");
            }

            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var email = HttpContext.Session.GetString("ResetEmail");
                    if (string.IsNullOrEmpty(email))
                    {
                        TempData["Error"] = "Oturum süresi dolmuş. Lütfen tekrar deneyin.";
                        return RedirectToAction("ForgotPassword");
                    }

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
                    if (user != null)
                    {
                        // Hash new password
                        user.Password = _authService.HashPassword(model.NewPassword);
                        user.UpdatedAt = DateTimeHelper.NowTurkey;
                        
                        // Clear any existing password reset data
                        user.PasswordResetToken = null;
                        user.PasswordResetTokenExpiry = null;
                        user.PasswordResetCode = null;
                        user.PasswordResetCodeExpiry = null;
                        
                        await _context.SaveChangesAsync();

                        // Clear session
                        HttpContext.Session.Remove("ResetEmail");

                        _logger.LogInformation("Password successfully changed for user: {Email}", email);
                        TempData["Success"] = "Şifreniz başarıyla güncellendi. Yeni şifrenizle giriş yapabilirsiniz.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        _logger.LogWarning("User not found for password change: {Email}", email);
                        ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ChangePassword for email: {Email}", HttpContext.Session.GetString("ResetEmail"));
                    ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();

                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                // Kullanıcının siparişlerini de al
                var userOrders = await _context.Orders
                    .Where(o => o.UserId == currentUser.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                ViewBag.UserOrders = userOrders;

                return View(currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                TempData["Error"] = "Profil yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Profile", model);
                }

                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login");
                }

                // Sadece güncellenebilir alanları güncelle
                currentUser.FirstName = model.FirstName?.Trim();
                currentUser.LastName = model.LastName?.Trim();
                currentUser.Phone = model.Phone?.Trim();
                currentUser.Address = model.Address?.Trim();
                currentUser.City = model.City?.Trim();
                currentUser.PostalCode = model.PostalCode?.Trim();
                currentUser.UpdatedAt = DateTimeHelper.NowTurkey;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Profile updated successfully for user: {UserId}", currentUser.Id);
                TempData["ProfileUpdateSuccess"] = "Profil bilgileriniz başarıyla güncellendi.";
                
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user");
                TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                return RedirectToAction("Profile");
            }
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Register");
            }

            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(string email, string verificationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode))
                {
                    ModelState.AddModelError("", "E-posta ve doğrulama kodu gereklidir.");
                    ViewBag.Email = email;
                    return View();
                }

                _logger.LogInformation("Email verification attempt for: {Email}", email);

                // Önce kullanıcının zaten doğrulanmış olup olmadığını kontrol et
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user != null && user.EmailConfirmed)
                {
                    _logger.LogInformation("Email already verified, redirecting to login: {Email}", email);
                    TempData["Success"] = "E-posta adresiniz zaten doğrulanmış! Giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }

                var result = await _authService.VerifyEmailAsync(email, verificationCode);
                if (result)
                {
                    _logger.LogInformation("Email verification successful for: {Email}", email);
                    TempData["Success"] = "E-posta adresiniz başarıyla doğrulandı! Artık giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }
                else
                {
                    _logger.LogWarning("Email verification failed for: {Email}", email);
                    
                    // Eğer kullanıcı zaten doğrulanmışsa ama bir şekilde buraya geldiyse
                    if (user != null && user.EmailConfirmed)
                    {
                        TempData["Success"] = "E-posta adresiniz zaten doğrulanmış! Giriş yapabilirsiniz.";
                        return RedirectToAction("Login");
                    }
                    
                    ModelState.AddModelError("", "Doğrulama kodu hatalı veya süresi dolmuş. Lütfen tekrar deneyin.");
                    ViewBag.Email = email;
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for: {Email}", email);
                ModelState.AddModelError("", "Doğrulama sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                ViewBag.Email = email;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "E-posta adresi gereklidir.";
                    return RedirectToAction("Register");
                }

                // Önce kullanıcının zaten doğrulanmış olup olmadığını kontrol et
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                if (user != null && user.EmailConfirmed)
                {
                    TempData["Success"] = "E-posta adresiniz zaten doğrulanmış! Giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("Resend verification code for: {Email}", email);

                var result = await _authService.ResendVerificationCodeAsync(email);
                if (result)
                {
                    TempData["Success"] = "Yeni doğrulama kodu e-posta adresinize gönderildi.";
                }
                else
                {
                    TempData["Error"] = "Doğrulama kodu gönderilemedi. Lütfen tekrar deneyin.";
                }

                return RedirectToAction("VerifyEmail", new { email = email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending verification code for: {Email}", email);
                TempData["Error"] = "Doğrulama kodu gönderilirken bir hata oluştu.";
                return RedirectToAction("VerifyEmail", new { email = email });
            }
        }
    }
}
