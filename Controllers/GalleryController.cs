using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public GalleryController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Gallery/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var galleries = await _context.Galleries
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.DisplayOrder)
                    .ToListAsync();

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
                
                return View(galleries);
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
                
                TempData["Error"] = "Galeri yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<Gallery>());
            }
        }
    }
} 