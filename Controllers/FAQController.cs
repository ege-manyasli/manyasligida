using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class FAQController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly ISiteSettingsService _siteSettingsService;

        public FAQController(ApplicationDbContext context, CartService cartService, ISiteSettingsService siteSettingsService)
        {
            _context = context;
            _cartService = cartService;
            _siteSettingsService = siteSettingsService;
        }

        // GET: FAQ/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var faqs = await _context.FAQs
                    .Where(f => f.IsActive)
                    .OrderBy(f => f.DisplayOrder)
                    .ToListAsync();

                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                
                var siteSettings = _siteSettingsService.Get();

                ViewBag.CartItemCount = _cartService.GetCartItemCount();
                ViewBag.Categories = categories;
                ViewBag.SiteSettings = siteSettings;
                
                return View(faqs);
            }
            catch (Exception ex)
            {
                var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
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