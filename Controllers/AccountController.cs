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

                var user = await _authService.LoginAsync(model.Email, model.Password);
                if (user != null)
                {
                    _logger.LogInformation("Login successful for: {Email}", model.Email);

                    // Create claims
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim("FullName", user.FullName),
                        new Claim("UserId", user.Id.ToString())
                    };

                    if (user.IsAdmin)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(1)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Set session manually (simplified)
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                    _logger.LogInformation("User {Email} logged in successfully", model.Email);
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogWarning("Login failed for: {Email}", model.Email);
                    // Check if user exists but email not confirmed
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive);
                    if (existingUser != null && !existingUser.EmailConfirmed)
                    {
                        ModelState.AddModelError("", "E-posta adresinizi doğrulamanız gerekmektedir. Lütfen e-posta kutunuzu kontrol edin ve doğrulama kodunu girin.");
                        TempData["UnverifiedEmail"] = model.Email;
                    }
                    else
                    {
                        ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                    }
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
                    return View(model);
                }

                _logger.LogInformation("Registration attempt for: {Email}", model.Email);

                var user = await _authService.RegisterAsync(model);
                if (user != null)
                {
                    _logger.LogInformation("Registration successful for: {Email}", model.Email);
                    TempData["Success"] = "Kayıt başarılı! E-posta adresinize gönderilen doğrulama kodunu girin.";
                    return RedirectToAction("VerifyEmail", new { email = model.Email });
                }
                else
                {
                    _logger.LogWarning("Registration failed for: {Email}", model.Email);
                    ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda veya kayıt sırasında bir hata oluştu.");
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
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                _logger.LogInformation("Logout attempt for user: {UserId}", userId);

                // Sign out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();

                _logger.LogInformation("User {UserId} logged out successfully", userId);
                TempData["Info"] = "Başarıyla çıkış yaptınız.";
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
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

                return View(currentUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile");
                TempData["Error"] = "Profil yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Test endpoint for debugging
        [HttpGet]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var userInfo = users.Select(u => new {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    FullName = u.FirstName + " " + u.LastName,
                    u.IsActive,
                    u.EmailConfirmed,
                    PasswordLength = u.Password?.Length ?? 0
                }).ToList();

                return Json(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugUsers");
                return Json(new { error = ex.Message });
            }
        }

        // Email Verification
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

        // Debug Email Verification
        [HttpGet]
        public async Task<IActionResult> DebugEmailVerification(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { error = "Email required" });
                }

                var verifications = await _context.EmailVerifications
                    .Where(v => v.Email.ToLower() == email.ToLower())
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(5)
                    .Select(v => new {
                        v.VerificationCode,
                        v.CreatedAt,
                        v.ExpiresAt,
                        v.IsUsed,
                        IsExpired = v.ExpiresAt <= DateTime.Now,
                        MinutesToExpire = (v.ExpiresAt - DateTime.Now).TotalMinutes
                    })
                    .ToListAsync();

                return Json(new { 
                    email = email,
                    verifications = verifications,
                    currentTime = DateTime.Now,
                    currentTimeUtc = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DebugEmailVerification");
                return Json(new { error = ex.Message });
            }
        }

        // Emergency test action
        [HttpGet]
        public IActionResult Test()
        {
            ViewBag.Message = "Account Controller çalışıyor! ✅";
            ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            ViewBag.UserName = User.Identity?.Name ?? "Anonim";
            return View();
        }
    }
}
