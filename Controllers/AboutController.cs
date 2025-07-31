using Microsoft.AspNetCore.Mvc;
using manyasligida.Services;
using manyasligida.Data;
using Microsoft.EntityFrameworkCore;

namespace manyasligida.Controllers
{
    public class AboutController : Controller
    {
        private readonly CartService _cartService;
        private readonly ApplicationDbContext _context;

        public AboutController(CartService cartService, ApplicationDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        // GET: About/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                
                // Site ayarları
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
                
                return View();
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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