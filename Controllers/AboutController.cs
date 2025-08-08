using Microsoft.AspNetCore.Mvc;
using manyasligida.Services;
using manyasligida.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace manyasligida.Controllers
{
    public class AboutController : Controller
    {
        private readonly CartService _cartService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISiteSettingsService _siteSettingsService;

        public AboutController(CartService cartService, IServiceProvider serviceProvider, ISiteSettingsService siteSettingsService)
        {
            _cartService = cartService;
            _serviceProvider = serviceProvider;
            _siteSettingsService = siteSettingsService;
        }

        // GET: About/Index
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
    }
} 