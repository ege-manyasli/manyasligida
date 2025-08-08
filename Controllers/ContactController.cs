using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class ContactController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public ContactController(IServiceProvider serviceProvider, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: Contact/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                
                var siteSettings = _siteSettingsService.Get();

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                return View();
            }
            catch (Exception)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = new
                {
                    Phone = "+90 266 123 45 67",
                    Email = "info@manyasligida.com",
                    Address = "Manyas, Balıkesir",
                    WorkingHours = "Pzt-Cmt: 08:00-18:00",
                    FacebookUrl = "#",
                    InstagramUrl = "#",
                    TwitterUrl = "#",
                    YoutubeUrl = "#"
                };
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                return View();
            }
        }

        // POST: Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessage model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    model.CreatedAt = DateTime.Now;
                    model.IsRead = false;
                    model.IsReplied = false;

                    context.ContactMessages.Add(model);
                    await context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Mesaj gönderilirken bir hata oluştu.");
            }

            using var errorScope = _serviceProvider.CreateScope();
            var errorContext = errorScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var categories = await errorContext.Categories.Where(c => c.IsActive).ToListAsync();
            var siteSettings = new
            {
                Phone = "+90 266 123 45 67",
                Email = "info@manyasligida.com",
                Address = "Manyas, Balıkesir",
                WorkingHours = "Pzt-Cmt: 08:00-18:00",
                FacebookUrl = "#",
                InstagramUrl = "#",
                TwitterUrl = "#",
                YoutubeUrl = "#"
            };

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            ViewBag.Categories = categories;
            ViewBag.SiteSettings = siteSettings;
            
            return View("Index", model);
        }
    }
} 