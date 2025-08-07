using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly IAuthService _authService;

        public AccountController(ApplicationDbContext context, CartService cartService, IAuthService authService)
        {
            _context = context;
            _cartService = cartService;
            _authService = authService;
        }

        // GET: Account/Login
        public async Task<IActionResult> Login()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _authService.LoginAsync(model.Email, model.Password);
                if (user != null)
                {
                    TempData["LoginSuccess"] = "Başarıyla giriş yaptınız!";
                    
                    // Admin ise admin panele yönlendir
                    if (user.IsAdmin)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Giriş sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // GET: Account/Register
        public async Task<IActionResult> Register()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                if (await _authService.IsEmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                    return View(model);
                }

                var user = await _authService.RegisterAsync(model);
                if (user != null)
                {
                    TempData["RegisterSuccess"] = "Kayıt işleminiz başarıyla tamamlandı! E-posta adresinize gönderilen doğrulama kodunu kullanarak hesabınızı doğrulayın.";
                    return RedirectToAction(nameof(VerifyEmail), new { email = user.Email });
                }
                else
                {
                    ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["LogoutSuccess"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Get user's order history
            var userOrders = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.UserOrders = userOrders;
            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View(user);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Phone = model.Phone;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.PostalCode = model.PostalCode;
                    user.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    TempData["ProfileUpdateSuccess"] = "Profil bilgileriniz güncellendi.";
                    
                    // Session'daki kullanıcı adını güncelle
                    _authService.SetCurrentUser(user);
                }
                catch (Exception)
                {
                    TempData["ProfileUpdateError"] = "Profil güncellenirken bir hata oluştu.";
                }
            }

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            return View("Profile", user);
        }

        // GET: Account/VerifyEmail
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new EmailVerificationViewModel
            {
                Email = email
            };

            return View(model);
        }

        // POST: Account/VerifyEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(EmailVerificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isVerified = await _authService.VerifyEmailAsync(model.Email, model.VerificationCode);
                if (isVerified)
                {
                    TempData["EmailVerificationSuccess"] = "E-posta adresiniz başarıyla doğrulandı! Şimdi giriş yapabilirsiniz.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError("VerificationCode", "Doğrulama kodu hatalı veya süresi dolmuş.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Doğrulama sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        // POST: Account/ResendVerificationCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            try
            {
                var isSent = await _authService.ResendVerificationCodeAsync(email);
                if (isSent)
                {
                    TempData["ResendSuccess"] = "Yeni doğrulama kodu e-posta adresinize gönderildi.";
                }
                else
                {
                    TempData["ResendError"] = "Doğrulama kodu gönderilemedi. Lütfen tekrar deneyin.";
                }
            }
            catch (Exception)
            {
                TempData["ResendError"] = "Doğrulama kodu gönderilemedi. Lütfen tekrar deneyin.";
            }

            return RedirectToAction(nameof(VerifyEmail), new { email });
        }
    }
} 