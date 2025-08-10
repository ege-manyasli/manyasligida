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

                    // Clear any existing session data first to prevent conflicts
                    HttpContext.Session.Clear();
                    
                    // Create unique session for this user/device combination
                    var sessionManager = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
                    await sessionManager.CreateUserSessionAsync(user);
                    
                    // Set session data with unique identifiers
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserName", user.FullName);
                    HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());
                    HttpContext.Session.SetString("LoginTime", DateTimeHelper.NowTurkeyString("yyyy-MM-dd HH:mm:ss"));
                    HttpContext.Session.SetString("DeviceId", Guid.NewGuid().ToString()); // Unique device identifier

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

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Clear session
                HttpContext.Session.Clear();
                
                // Invalidate session in database
                var sessionManager = HttpContext.RequestServices.GetRequiredService<ISessionManager>();
                await sessionManager.InvalidateSessionAsync();
                
                _logger.LogInformation("User logged out successfully");
                
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    // AuthService'i kullanarak şifre sıfırlama kodu gönder
                    var emailSent = await authService.SendPasswordResetCodeAsync(model.Email);
                    
                    if (emailSent)
                    {
                        _logger.LogInformation("Şifre sıfırlama e-postası başarıyla gönderildi: {Email}", model.Email);
                        TempData["Success"] = "Şifre sıfırlama kodu e-posta adresinize gönderildi.";
                        TempData["Email"] = model.Email; // For next step
                        return RedirectToAction("VerifyResetCode");
                    }
                    else
                    {
                        _logger.LogWarning("Şifre sıfırlama e-postası gönderilemedi: {Email}", model.Email);
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

        // GET: Account/VerifyResetCode
        public IActionResult VerifyResetCode()
        {
            var email = TempData["Email"]?.ToString();
            var model = new VerifyResetCodeViewModel
            {
                Email = email ?? string.Empty
            };
            return View(model);
        }

        // POST: Account/VerifyResetCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    // AuthService'i kullanarak şifre sıfırlama kodunu doğrula
                    var isValid = await authService.VerifyPasswordResetCodeAsync(model.Email, model.VerificationCode);

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

        // GET: Account/ChangePassword
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

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using var scope = HttpContext.RequestServices.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var email = HttpContext.Session.GetString("ResetEmail");
                    if (string.IsNullOrEmpty(email))
                    {
                        TempData["Error"] = "Oturum süresi dolmuş. Lütfen tekrar deneyin.";
                        return RedirectToAction("ForgotPassword");
                    }

                    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
                    if (user != null)
                    {
                        // Hash new password
                        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                        user.Password = authService.HashPassword(model.NewPassword);
                        user.UpdatedAt = DateTimeHelper.NowTurkey;
                        
                        // Clear any existing password reset data
                        user.PasswordResetToken = null;
                        user.PasswordResetTokenExpiry = null;
                        user.PasswordResetCode = null;
                        user.PasswordResetCodeExpiry = null;
                        
                        await context.SaveChangesAsync();

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

        // Debug Email Verification - DETAYLI
        [HttpGet]
        public async Task<IActionResult> DebugEmailVerification(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { error = "Email required" });
                }

                // Kullanıcı bilgilerini al
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                var verifications = await _context.EmailVerifications
                    .Where(v => v.Email.ToLower() == email.ToLower())
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(5)
                    .Select(v => new {
                        v.VerificationCode,
                        v.CreatedAt,
                        v.ExpiresAt,
                        v.IsUsed,
                        IsExpired = v.ExpiresAt <= DateTimeHelper.NowTurkey,
                        MinutesToExpire = (v.ExpiresAt - DateTimeHelper.NowTurkey).TotalMinutes
                    })
                    .ToListAsync();

                return Json(new { 
                    email = email,
                    user = user != null ? new {
                        user.Id,
                        user.Email,
                        user.EmailConfirmed,
                        user.IsActive,
                        user.CreatedAt
                    } : null,
                    verifications = verifications,
                    currentTime = DateTimeHelper.NowTurkey,
                    currentTimeUtc = DateTimeHelper.NowTurkey,
                    debugInfo = new {
                        userExists = user != null,
                        emailConfirmed = user?.EmailConfirmed ?? false,
                        activeVerifications = verifications.Count(v => !v.IsUsed),
                        usedVerifications = verifications.Count(v => v.IsUsed),
                        expiredVerifications = verifications.Count(v => v.IsExpired)
                    }
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

        [HttpGet]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                var testEmail = "test@example.com";
                var testCode = "123456";
                
                _logger.LogInformation("Test e-postası gönderiliyor...");
                var result = await emailService.SendVerificationEmailAsync(testEmail, testCode);
                
                var response = new
                {
                    Success = result,
                    Message = result ? "Test e-postası başarıyla gönderildi" : "Test e-postası gönderilemedi",
                    Timestamp = DateTime.Now,
                    TestEmail = testEmail,
                    TestCode = testCode,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                };
                
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test e-postası hatası");
                
                var response = new
                {
                    Success = false,
                    Message = $"Test e-postası hatası: {ex.Message}",
                    Error = ex.ToString(),
                    Timestamp = DateTime.Now,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
                };
                
                return Json(response);
            }
        }

        [HttpGet]
        public IActionResult EmailConfig()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                
                var smtpServer = configuration["EmailSettings:SmtpServer"];
                var smtpPort = configuration["EmailSettings:SmtpPort"];
                var smtpUsername = configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = configuration["EmailSettings:SmtpPassword"];
                var fromEmail = configuration["EmailSettings:FromEmail"];
                var fromName = configuration["EmailSettings:FromName"];
                var environment = configuration["ASPNETCORE_ENVIRONMENT"];
                var enableSsl = configuration["EmailSettings:EnableSsl"];
                var useDefaultCredentials = configuration["EmailSettings:UseDefaultCredentials"];
                var timeout = configuration["EmailSettings:Timeout"];
                var deliveryMethod = configuration["EmailSettings:DeliveryMethod"];
                
                var configInfo = new
                {
                    SmtpServer = smtpServer,
                    SmtpPort = smtpPort,
                    SmtpUsername = smtpUsername,
                    SmtpPassword = string.IsNullOrEmpty(smtpPassword) ? "BOŞ" : "AYARLANMIŞ",
                    FromEmail = fromEmail,
                    FromName = fromName,
                    Environment = environment,
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = useDefaultCredentials,
                    Timeout = timeout,
                    DeliveryMethod = deliveryMethod,
                    HasAllSettings = !string.IsNullOrEmpty(smtpServer) && 
                                   !string.IsNullOrEmpty(smtpPort) && 
                                   !string.IsNullOrEmpty(smtpUsername) && 
                                   !string.IsNullOrEmpty(smtpPassword) && 
                                   !string.IsNullOrEmpty(fromEmail),
                    CurrentTime = DateTime.Now,
                    CurrentTimeUtc = DateTime.UtcNow,
                    ServerTimeZone = TimeZoneInfo.Local.DisplayName
                };
                
                return Json(configInfo);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestGmailConnection()
        {
            try
            {
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
                
                var smtpServer = configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = configuration["EmailSettings:SmtpPassword"];
                var enableSsl = configuration.GetValue<bool>("EmailSettings:EnableSsl", true);
                var timeout = configuration.GetValue<int>("EmailSettings:Timeout", 60000);

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                using var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                    Timeout = timeout,
                    DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                };

                // Test connection
                await smtpClient.SendMailAsync(new System.Net.Mail.MailMessage());
                
                stopwatch.Stop();
                
                var response = new
                {
                    Success = true,
                    Message = "Gmail SMTP bağlantısı başarılı",
                    SmtpServer = smtpServer,
                    Port = smtpPort,
                    SslEnabled = enableSsl,
                    ConnectionTime = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.Now
                };
                
                return Json(response);
            }
            catch (Exception ex)
            {
                var suggestions = "";
                if (ex.Message.Contains("Authentication"))
                {
                    suggestions = "1. Gmail'de 2FA'yı etkinleştirin<br>2. App Password oluşturun<br>3. App Password'ü kullanın";
                }
                else if (ex.Message.Contains("timeout"))
                {
                    suggestions = "1. İnternet bağlantınızı kontrol edin<br>2. Firewall ayarlarını kontrol edin<br>3. Port 587 veya 465'nin açık olduğundan emin olun";
                }
                else if (ex.Message.Contains("SSL"))
                {
                    suggestions = "1. SSL ayarlarını kontrol edin<br>2. Port 465 (SSL) veya 587 (TLS) kullanın";
                }
                
                var response = new
                {
                    Success = false,
                    Message = $"Gmail SMTP bağlantısı başarısız: {ex.Message}",
                    Error = ex.ToString(),
                    Suggestions = suggestions,
                    Timestamp = DateTime.Now
                };
                
                return Json(response);
            }
        }
    }
}
