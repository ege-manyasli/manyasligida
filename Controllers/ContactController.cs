using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using manyasligida.Services;

namespace manyasligida.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;

        public ContactController(ApplicationDbContext context, CartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // GET: Contact/Index
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
            catch (Exception)
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

        // POST: Contact/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(ContactMessage model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.CreatedAt = DateTime.Now;
                    model.IsRead = false;
                    model.IsReplied = false;

                    _context.ContactMessages.Add(model);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "Mesajınız başarıyla gönderildi. En kısa sürede size dönüş yapacağız.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Mesaj gönderilirken bir hata oluştu.");
            }

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

            ViewBag.CartItemCount = _cartService.GetCartItemCount();
            ViewBag.Categories = categories;
            ViewBag.SiteSettings = siteSettings;
            
            return View("Index", model);
        }
    }
} 