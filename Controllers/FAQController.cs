using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public FAQController(IServiceProvider serviceProvider, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _serviceProvider = serviceProvider;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: FAQ/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var faqs = await context.FAQs
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                
                var siteSettings = _siteSettingsService.Get();

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                return View(faqs);
            }
            catch (Exception ex)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = _siteSettingsService.Get();
                
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                
                TempData["Error"] = "SSS yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<FAQ>());
            }
        }
    }
} 