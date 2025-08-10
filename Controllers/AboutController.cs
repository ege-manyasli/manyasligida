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
                var aboutService = scope.ServiceProvider.GetRequiredService<manyasligida.Services.Interfaces.IAboutService>();

                var categories = await context.Categories.Where(c => c.IsActive).ToListAsync();
                var siteSettings = await _siteSettingsService.GetAsync();

                // Get about content from database
                var aboutResult = await aboutService.GetAboutContentAsync();
                
                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                if (aboutResult.Success && aboutResult.Data != null)
                {
                    // Add cache busting to image URLs
                    var aboutContent = aboutResult.Data;
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    
                    // Add timestamp to image URLs to prevent caching
                    if (!string.IsNullOrEmpty(aboutContent.StoryImageUrl))
                    {
                        aboutContent = aboutContent with { 
                            StoryImageUrl = $"{aboutContent.StoryImageUrl}?v={timestamp}" 
                        };
                    }
                    
                    if (!string.IsNullOrEmpty(aboutContent.RegionImageUrl))
                    {
                        aboutContent = aboutContent with { 
                            RegionImageUrl = $"{aboutContent.RegionImageUrl}?v={timestamp}" 
                        };
                    }
                    
                    ViewBag.AboutContent = aboutContent;
                }
                else
                {
                    // Create default content if none exists
                    var defaultResult = await aboutService.CreateDefaultAboutContentAsync();
                    ViewBag.AboutContent = defaultResult.Data;
                }
                
                return View();
            }
            catch (Exception ex)
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
                ViewBag.AboutContent = null; // Fallback to static content
                
                return View();
            }
        }
    }
} 